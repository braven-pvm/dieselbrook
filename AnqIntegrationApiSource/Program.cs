using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Middleware;
using AnqIntegrationApi.Models.Settings;
using AnqIntegrationApi.Services.Messaging;
using AnqIntegrationApi.Services.Outbox;
using AnqIntegrationApi.Services.WhatsAppOptin;
using AnqIntegrationApi.Services.Workers;
using AnqIntegrationApi.Workers;
using BrevoApiHelpers.Models;
using BrevoApiHelpers.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.IO;                 // ⬅ added
using System.Net.Http.Headers;
using System.Reflection;          // ⬅ added
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)     // <-- this makes appsettings overrides work
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "logs/brevo-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {SourceContext} {Message}{NewLine}{Exception}"
    )
        .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();


// MVC
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();



// Swagger + Bearer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("public-v1", new OpenApiInfo { Title = "AnqIntegrationApi", Version = "v1" });
    c.SwaggerDoc("internal-v1", new OpenApiInfo { Title = "AnqIntegrationApi (Internal)", Version = "v1" });

    // File upload support
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var group = apiDesc.GroupName ?? "public";
        return docName == "public-v1" ? group != "internal" : group == "internal";
    });

    // ✅ Make [SwaggerOperation]/[SwaggerResponse] work
    c.EnableAnnotations();

    // ✅ Pull in <summary> and <remarks> from XML docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // -------------------------
    // JWT Bearer (existing)
    // -------------------------
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token only",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", bearerScheme);

    // -------------------------
    // X-Api-Key (new)
    // -------------------------
    var apiKeyScheme = new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Description = "Enter the static API key (sent as header X-Api-Key)",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
    };

    c.AddSecurityDefinition("ApiKey", apiKeyScheme);

    // -------------------------
    // Security Requirements
    // IMPORTANT:
    // Add as separate requirements so Swagger shows BOTH in Authorize dialog.
    // Users can fill either or both.
    // -------------------------
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { bearerScheme, Array.Empty<string>() }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { apiKeyScheme, Array.Empty<string>() }
    });
});




// Http + Context
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<OutboxDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("BrevoDb")));

builder.Services.Configure<OutboxMessagingOptions>(configuration.GetSection("OutboxMessaging"));
// DI
builder.Services.AddSingleton<IOutboxPolicy, OutboxPolicy>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IMessagingDispatcher, MessagingDispatcher>();
builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor>();

builder.Services.AddScoped<NopDbContext>(sp =>
{
    var http = sp.GetRequiredService<IHttpContextAccessor>();
    var ctx = http.HttpContext;

    var conn = ctx?.Items["NopDbConnection"] as string;

    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("NopDbConnection not available. Ensure ApiClientContextMiddleware resolved the client (JWT or X-Api-Key).");

    var options = new DbContextOptionsBuilder<NopDbContext>()
        .UseSqlServer(conn, sql =>
        {
            sql.EnableRetryOnFailure();
            sql.CommandTimeout(60);
        })
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        .Options;

    return new NopDbContext(options);
});

// Background worker (toggle via config)
var outboxEnabled = builder.Configuration.GetValue<bool?>("Outbox:BackgroundWorkerEnabled") ?? true;
if (outboxEnabled)
{
    builder.Services.AddHostedService<BrevoOutboxWorker>();
}


// Brevo settings
builder.Services.Configure<BrevoSettings>(configuration.GetSection("Brevo"));

// Named HttpClients
builder.Services.AddHttpClient("Brevo", (sp, client) =>
{
    var brevo = sp.GetRequiredService<IOptions<BrevoSettings>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(brevo.BaseUrl) ? "https://api.brevo.com/v3/" : brevo.BaseUrl;
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    if (!string.IsNullOrWhiteSpace(brevo.ApiKey))
        client.DefaultRequestHeaders.Add("api-key", brevo.ApiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("Default", (sp, client) =>
{
    var brevo = sp.GetRequiredService<IOptions<BrevoSettings>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(brevo.BaseUrl) ? "https://api.brevo.com/v3/" : brevo.BaseUrl;
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    if (!string.IsNullOrWhiteSpace(brevo.ApiKey))
        client.DefaultRequestHeaders.Add("api-key", brevo.ApiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient(Options.DefaultName, (sp, client) =>
{
    var brevo = sp.GetRequiredService<IOptions<BrevoSettings>>().Value;
    var baseUrl = string.IsNullOrWhiteSpace(brevo.BaseUrl) ? "https://api.brevo.com/v3/" : brevo.BaseUrl;
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    if (!string.IsNullOrWhiteSpace(brevo.ApiKey))
        client.DefaultRequestHeaders.Add("api-key", brevo.ApiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// App settings DB (static connection for settings)
builder.Services.AddDbContext<ApiSettingsDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("Settings")));

// Dynamic client factories and services
builder.Services.AddScoped<AnqIntegrationApi.Services.IClientDbContextFactory, AnqIntegrationApi.Services.ClientDbContextFactory>();
builder.Services.AddScoped<AnqIntegrationApi.Services.ISyncService, AnqIntegrationApi.Services.SyncService>();

// Brevo helper services
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
builder.Services.AddScoped<IConversationService, ConversationService>();



// Brevo Events
var eventsSyncEnabled = builder.Configuration.GetValue<bool?>("BrevoEventsSync:Enabled") ?? true;
if (eventsSyncEnabled)
{
    builder.Services.AddHostedService<BrevoWhatsappEventsWorker>();
}
var optinWorkerEnabled = builder.Configuration.GetValue<bool?>("WhatsAppOptinEmailWorker:Enabled") ?? true;
if (optinWorkerEnabled && !builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<WhatsAppOptinEmailWorker>();
}
builder.Services.AddScoped<IWhatsAppOptinSendQueueRunner, WhatsAppOptinSendQueueRunner>();


// JWT Auth
var jwtSection = configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SigningKey"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://annique.com",
                "https://www.annique.com",
                "http://127.0.0.1",
                "https://127.0.0.1",
                "http://127.0.0.1:5501",
                "http://localhost:5501",
                "https://localhost:54360"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // If you use cookies: add .AllowCredentials();
    });
});




var app = builder.Build();

var brevoCs = app.Configuration.GetConnectionString("BrevoDb");
Console.WriteLine($"ConnectionStrings:Brevo length = {brevoCs?.Length ?? 0}");
Console.WriteLine($"ConnectionStrings:Brevo is null/empty = {string.IsNullOrWhiteSpace(brevoCs)}");

var env = app.Environment;
var cfg = app.Configuration;

Console.WriteLine($"ENV: {env.EnvironmentName}");
Console.WriteLine($"ContentRootPath: {env.ContentRootPath}");
Console.WriteLine($"WebRootPath: {env.WebRootPath}");

Console.WriteLine("Config sources:");
if (cfg is IConfigurationRoot root)
{
    foreach (var p in root.Providers)
        Console.WriteLine(" - " + p);
}



// Serve JSON at /swagger/{documentName}/swagger.json
app.UseSwagger();

// Serve UI at /swagger
app.UseSwaggerUI(ui =>
{
    ui.SwaggerEndpoint("public-v1/swagger.json", "Public API");

    if (!app.Environment.IsProduction())
        ui.SwaggerEndpoint("internal-v1/swagger.json", "Internal API");

    ui.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("CorsPolicy");   // ✅ must be before auth
app.UseApiClientContext();
app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();

app.Run();

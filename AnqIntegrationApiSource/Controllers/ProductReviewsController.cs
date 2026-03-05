using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Nop;
using AnqIntegrationApi.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnqIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductReviewsController : ControllerBase
{
    private readonly ApiSettingsDbContext _settingsDb;
    private readonly IHttpContextAccessor _http;

    public ProductReviewsController(ApiSettingsDbContext settingsDb, IHttpContextAccessor http)
    {
        _settingsDb = settingsDb;
        _http = http;
    }

    private async Task<NopDbContext> CreateNopDbAsync(CancellationToken ct)
    {
        var client = _http.HttpContext?.Items["ApiClient"] as ApiClient;
        if (client == null || string.IsNullOrWhiteSpace(client.NopDbConnection))
            throw new UnauthorizedAccessException("Invalid API client context");

        var opts = new DbContextOptionsBuilder<NopDbContext>()
            .UseSqlServer(client.NopDbConnection)
            .Options;

        return new NopDbContext(opts);
    }

    // -----------------------------
    // Customer lookup
    // -----------------------------
    [HttpGet("customers/by-username")]
    public async Task<IActionResult> GetCustomerByUsername(string username, CancellationToken ct)
    {
        await using var db = await CreateNopDbAsync(ct);

        var customer = await db.Customers
            .AsNoTracking()
            .Where(c =>
                !c.Deleted &&
                c.Active &&
                !c.IsSystemAccount &&
                (c.Username == username || c.Email == username))
            .Select(c => new
            {
                c.Id,
                c.Username,
                c.Email,
                c.FirstName,
                c.LastName
            })
            .FirstOrDefaultAsync(ct);

        return customer == null ? NotFound() : Ok(customer);
    }

    // -----------------------------
    // Product lookup
    // -----------------------------
    [HttpGet("products/by-sku")]
    public async Task<IActionResult> GetProductBySku(string sku, CancellationToken ct)
    {
        await using var db = await CreateNopDbAsync(ct);

        var product = await db.Product
            .AsNoTracking()
            .Where(p =>
                !p.Deleted &&
                p.Published &&
              //  p.AllowCustomerReviews==1 &&
                p.Sku == sku)
            .Select(p => new
            {
                p.Id,
                p.Sku,
                p.Name,
                p.Deleted,
                p.Published,
                p.AllowCustomerReviews
            })
            .FirstOrDefaultAsync(ct);

        return product == null ? NotFound() : Ok(product);
    }

    // -----------------------------
    // Duplicate check
    // -----------------------------
    [HttpPost("check-duplicate")]
    public async Task<IActionResult> CheckDuplicate(ProductReview review, CancellationToken ct)
    {
        await using var db = await CreateNopDbAsync(ct);

        var exists = await db.ProductReview
            .AsNoTracking()
            .AnyAsync(r =>
                r.CustomerId == review.CustomerId &&
                r.ProductId == review.ProductId &&
                r.Title == review.Title &&
                r.ReviewText == review.ReviewText,
                ct);

        return Ok(new { exists });
    }

    // -----------------------------
    // Manual insert
    // -----------------------------
    [HttpPost("manual")]
    public async Task<IActionResult> CreateManualReview(ProductReview review, CancellationToken ct)
    {
        await using var db = await CreateNopDbAsync(ct);

        if (review.CustomerId <= 0 || review.ProductId <= 0)
            return BadRequest("CustomerId and ProductId required");

        if (string.IsNullOrWhiteSpace(review.Title) ||
            string.IsNullOrWhiteSpace(review.ReviewText))
            return BadRequest("Title and ReviewText required");

        // Defaults / safety
        review.Id = 0; // ensure insert
        review.StoreId = review.StoreId == 0 ? 1 : review.StoreId;
        review.IsApproved = review.IsApproved;
        review.Rating = review.Rating == 0 ? 5 : review.Rating;
        review.HelpfulYesTotal = 0;
        review.HelpfulNoTotal = 0;
        review.CustomerNotifiedOfReply = false;
        review.CreatedOnUtc = review.CreatedOnUtc == default
            ? DateTime.UtcNow
            : review.CreatedOnUtc;

        var duplicate = await db.ProductReview
            .AnyAsync(r =>
                r.CustomerId == review.CustomerId &&
                r.ProductId == review.ProductId &&
                r.Title == review.Title &&
                r.ReviewText == review.ReviewText,
                ct);

        if (duplicate)
            return Conflict("Duplicate review exists");

        db.ProductReview.Add(review);
        await db.SaveChangesAsync(ct);

        return Ok(new
        {
            review.Id,
            review.CustomerId,
            review.ProductId
        });
    }
}

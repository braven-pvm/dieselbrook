using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class JwtTestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public JwtTestController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Generates a test JWT token using appsettings.json config and request values.
        /// </summary>
        [HttpPost("generate")]
        public IActionResult GenerateTestToken([FromBody] TestTokenRequest request)
        {
            var jwt = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SigningKey"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.ClientKey),
                new Claim("ClientKey", request.ClientKey)
            };

            foreach (var role in request.Roles.Split(','))
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));

            var expires = request.NoExpiry
                ? DateTime.UtcNow.AddYears(10)
                : DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwt["ExpireMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expires,
                roles = request.Roles.Split(',').Select(r => r.Trim()).ToArray()
            });
        }
    }

    public class TestTokenRequest
    {
        public string ClientKey { get; set; } = "test-client";
        public string Roles { get; set; } = "Admin,SyncUser";
        public bool NoExpiry { get; set; } = false;
    }
}
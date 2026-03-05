using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AnqIntegrationApi.Controllers
{
    /// <summary>
    /// Returns info about the currently authenticated client.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class MeController : ControllerBase
    {
        /// <summary>
        /// Returns the authenticated client's key and roles.
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult GetCurrentClient()
        {
            var clientKey = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var roles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToArray();
            Console.WriteLine($"CLIENT: {clientKey}");
            foreach (var claim in User.Claims)
            {
       
                Console.WriteLine($"CLAIM: {claim.Type} = {claim.Value}");
            }


            return Ok(new
            {
                clientKey,
                roles
            });
        }
    }
}
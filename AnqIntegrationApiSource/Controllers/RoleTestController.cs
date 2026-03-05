using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnqIntegrationApi.Controllers
{
    /// <summary>
    /// Demonstrates role-based access control using JWT claims.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiExplorerSettings(GroupName = "internal")]
    public class RoleTestController : ControllerBase
    {
        /// <summary>
        /// This endpoint is accessible only by users with the "Admin" role.
        /// </summary>
        /// <returns>A string confirming admin access.</returns>
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok("✅ You have the Admin role.");
        }

        /// <summary>
        /// This endpoint allows both "User" and "Admin" roles.
        /// </summary>
        /// <returns>A string confirming access for user or admin.</returns>
        [HttpGet("user-or-admin")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult UserOrAdmin()
        {
            return Ok("✅ You have either User or Admin role.");
        }

        /// <summary>
        /// This endpoint is public and does not require authentication.
        /// </summary>
        /// <returns>A public message.</returns>
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok("🌐 This endpoint is public.");
        }
    }
}
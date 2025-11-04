// -----------------------------------------------------------------------------
// File: AuthController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Provides a basic authentication endpoint for development and testing.
//   Generates a JSON Web Token (JWT) with a hardcoded Admin claim.
//   JWT configuration is read from appsettings.json.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        // ---------------------------------------------------------------------
        // Constructor: Injects configuration and hosting environment
        // ---------------------------------------------------------------------
        public AuthController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        // Record type used for simple login payload (email + password)
        public record LoginDto(string Email, string Password);

        // ---------------------------------------------------------------------
        // POST: /api/auth/login
        // Purpose:
        //   Issues a JWT for development use only.
        //   The token includes email and role claims.
        // Notes:
        //   • This endpoint is disabled in production environments.
        //   • JWT settings are loaded from appsettings.json > "Jwt" section.
        //   • Token expires in 2 hours.
        // ---------------------------------------------------------------------
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            // Prevent this endpoint from being used outside development
            if (!_env.IsDevelopment())
                return Forbid(); // Return 403 Forbidden in production

            // Load JWT configuration values
            var jwt = _config.GetSection("Jwt");

            // Create symmetric security key using secret from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            // Create signing credentials using HMAC SHA-256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define claims (user identity information)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, dto.Email ?? "admin@bobatastic.local"),
                new Claim(ClaimTypes.Email, dto.Email ?? "admin@bobatastic.local"),
                new Claim(ClaimTypes.Role, "Admin")  // Assign Admin role for test login
            };

            // Build the JWT token
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            // Convert token to string for client use
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return token to client as JSON response
            return Ok(new { token = tokenString });
        }
    }
}

// ---------------------------------------------------------------
// File: AuthController.cs  (DEV-ONLY token issuer for assessment)
// ---------------------------------------------------------------
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

        public AuthController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public record LoginDto(string Email, string Password);

        // POST: /api/auth/login
        // DEV ONLY: accepts any email/password and issues an Admin token in Development.
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (!_env.IsDevelopment())
                return Forbid(); // lock in prod

            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, dto.Email ?? "admin@bobatastic.local"),
                new Claim(ClaimTypes.Email, dto.Email ?? "admin@bobatastic.local"),
                new Claim(ClaimTypes.Role, "Admin") // <-- give Admin role for testing
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenString });
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BobaShop.Api.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public AuthController(IConfiguration config,
                              UserManager<ApplicationUser> users,
                              SignInManager<ApplicationUser> signIn)
        {
            _config = config;
            _users = users;
            _signIn = signIn;
        }

        // DTOs kept INSIDE the class to avoid top-level statements
        public record RegisterRequest(string Email, string Password, string ConfirmPassword, string Name);
        public record LoginRequest(string Email, string Password);

        private string CreateJwtToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new("fullName", user.FullName ?? string.Empty)
            };
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (req is null) return BadRequest(new { error = "Invalid payload." });
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.ConfirmPassword) ||
                string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { error = "Email, Password, ConfirmPassword and Name are required." });

            if (req.Password != req.ConfirmPassword)
                return BadRequest(new { error = "Passwords do not match." });

            var existing = await _users.FindByEmailAsync(req.Email);
            if (existing != null)
                return Conflict(new { error = "Email is already registered." });

            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email,
                FullName = req.Name,
                RewardPoints = 0
            };

            var create = await _users.CreateAsync(user, req.Password);
            if (!create.Succeeded)
                return BadRequest(new { errors = create.Errors.Select(e => e.Description) });

            try { await _users.AddToRoleAsync(user, "Customer"); } catch { /* ok if roles not seeded */ }

            return Created($"/api/users/{user.Id}", new { id = user.Id, email = user.Email, message = "User registered successfully" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (req is null) return BadRequest(new { error = "Invalid payload." });

            var user = await _users.FindByEmailAsync(req.Email);
            if (user is null)
                return BadRequest(new { error = "Invalid email or password." });

            var valid = await _signIn.CheckPasswordSignInAsync(user, req.Password, false);
            if (!valid.Succeeded)
                return BadRequest(new { error = "Invalid email or password." });

            var roles = await _users.GetRolesAsync(user);
            if (roles == null || roles.Count == 0) roles = new List<string> { "Customer" };

            var token = CreateJwtToken(user, roles);

            return Ok(new
            {
                token,
                user = new { id = user.Id, email = user.Email, name = user.FullName, roles },
                expiresInSeconds = 7200
            });
        }
    }
}

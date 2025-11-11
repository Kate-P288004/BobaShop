// -----------------------------------------------------------------------------
// File: Controllers/AuthController.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Issue JWTs that include role claims so admin-only endpoints work.
//   Endpoints:
//     POST /api/v1/Auth/register
//     POST /api/v1/Auth/login
// Notes:
//   - Program.cs maps RoleClaimType = "role" and NameClaimType = "name".
//   - This controller sets those claims accordingly.
// -----------------------------------------------------------------------------

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using BobaShop.Api.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Asp.Versioning.ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public AuthController(
            IConfiguration config,
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn)
        {
            _config = config;
            _users = users;
            _signIn = signIn;
        }

        // Keep DTOs scoped to this controller.
        public record RegisterRequest(string Email, string Password, string ConfirmPassword, string Name);
        public record LoginRequest(string Email, string Password);

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------
        private string CreateJwtToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims aligned to Program.cs:
            //  - "role"  -> RoleClaimType
            //  - "name"  -> NameClaimType
            //  - "email" and "sub" are standard and useful for clients
            var claims = new List<Claim>
            {
                new("sub", user.Id),
                new("name", user.UserName ?? user.Email ?? string.Empty),
                new("email", user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var r in roles)
                claims.Add(new Claim("role", r));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4), // 4h session
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ---------------------------------------------------------------------
        // POST /api/v1/Auth/register
        // Registers a new user and assigns them the "Customer" role.
        // Returns 201 Created with basic user info (no token issued here).
        // ---------------------------------------------------------------------
        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (req is null)
                return BadRequest(new { error = "Invalid payload." });

            var email = (req.Email ?? string.Empty).Trim();
            var name = (req.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.ConfirmPassword) ||
                string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { error = "Email, Password, ConfirmPassword and Name are required." });
            }

            if (!email.Contains('@'))
                return BadRequest(new { error = "Please provide a valid email address." });

            if (req.Password != req.ConfirmPassword)
                return BadRequest(new { error = "Passwords do not match." });

            var existing = await _users.FindByEmailAsync(email);
            if (existing != null)
                return Conflict(new { error = "Email is already registered." });

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                RewardPoints = 0
            };

            var create = await _users.CreateAsync(user, req.Password);
            if (!create.Succeeded)
                return BadRequest(new { errors = create.Errors.Select(e => e.Description) });

            // Assign default role (ignore if roles not seeded yet)
            try { await _users.AddToRoleAsync(user, "Customer"); } catch { /* ignore */ }

            // Location is a simple logical pointer (you can add a UsersController later)
            return Created($"/api/v1/users/{user.Id}", new
            {
                id = user.Id,
                email = user.Email,
                name = user.FullName,
                message = "User registered successfully."
            });
        }

        // ---------------------------------------------------------------------
        // POST /api/v1/Auth/login
        // Authenticates with email/password and returns an access_token (JWT).
        // The JWT includes "role" claims so admin policy works.
        // ---------------------------------------------------------------------
        [HttpPost("login")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (req is null)
                return BadRequest(new { error = "Invalid payload." });

            var email = (req.Email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { error = "Email and password are required." });

            var user = await _users.FindByEmailAsync(email);
            if (user is null)
                return Unauthorized(new { error = "Invalid email or password." });

            var valid = await _signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
            if (!valid.Succeeded)
                return Unauthorized(new { error = "Invalid email or password." });

            var roles = await _users.GetRolesAsync(user);
            if (roles is null || roles.Count == 0)
                roles = new List<string> { "Customer" };

            var token = CreateJwtToken(user, roles);

            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                // 4 hours = 14400 seconds (keep in sync with CreateJwtToken)
                expires_in = 14400,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.FullName,
                    roles
                }
            });
        }
    }
}

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
//   - Tokens expire in 4 hours (keep "expires_in" aligned with CreateJwtToken).
//   - DTOs are scoped locally to this controller to keep the public API surface
//     small and focused.
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
    //--------------------------------------------------------------------------
    // Controller: AuthController
    // Route base: /api/v{version}/Auth
    // Produces: application/json
    //
    // Why versioned? Allows future auth flows (e.g., refresh tokens, MFA)
    // without breaking existing clients: keep /v1 stable, add /v2 later.
    //
    // Security model (high level):
    // - Registration: open to anonymous users, creates Identity user,
    //   assigns default "Customer" role (best-effort).
    // - Login: open to anonymous users, returns signed JWT with:
    //     * sub  = user id
    //     * name = display name (or email fallback)
    //     * email
    //     * role = one or more role claims (e.g., "Admin", "Customer")
    // - API protects admin endpoints using [Authorize(Roles="Admin")].
    //--------------------------------------------------------------------------
    [ApiController]
    [Asp.Versioning.ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        // IConfiguration -> reads Jwt:Key/Issuer/Audience for token creation.
        // UserManager     -> creates and queries users.
        // SignInManager   -> validates credentials (password checks, lockout).
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
        // Rationale: These payloads are only meaningful for auth flows.
        // If you later expose user management endpoints, consider moving to a
        // shared Dto folder to avoid duplication.
        public record RegisterRequest(string Email, string Password, string ConfirmPassword, string Name);
        public record LoginRequest(string Email, string Password);

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------
        // CreateJwtToken:
        //   - Reads Jwt settings from configuration.
        //   - Signs with HMAC-SHA256 using Jwt:Key.
        //   - Embeds identity claims required by the app:
        //       "sub"   -> stable subject identifier (user id)
        //       "name"  -> display name for UI convenience
        //       "email" -> email address for client display/debugging
        //       "role"  -> one claim per role (enables [Authorize(Roles=...)] )
        //       "jti"   -> unique token id (helps with log correlation/revocation)
        //   - Lifetime: 4 hours (also returned to client as expires_in=14400).
        //   - Issuer/Audience must match those configured in Program.cs.
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

            // One claim per role keeps things simple for policy evaluation and
            // matches ASP.NET Core's expectations for role-based authorization.
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
        //
        // Validation:
        // - Email present and syntactically valid (basic '@' check).
        // - Password and ConfirmPassword must match (complexity enforced by
        //   Identity options in Program.cs).
        //
        // Behavior notes:
        // - If email already exists -> 409 Conflict (prevents duplicate users).
        // - Roles may not yet be seeded; AddToRoleAsync is wrapped in try/catch
        //   so registration still succeeds (user can receive role later).
        // - Location header (Created) points to future /users/{id} resource.
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

            // Assign default role (ignore if roles not seeded yet).
            // This keeps registration resilient on first run; a later seeding
            // pass or admin action can add roles without blocking new users.
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
        //
        // Failure modes:
        // - 400 BadRequest: missing email or password
        // - 401 Unauthorized: email not found or password invalid
        //
        // Success payload:
        // - access_token: the JWT for Authorization: Bearer {token}
        // - token_type:   "Bearer" (OAuth2-style convention)
        // - expires_in:   14400 seconds (4 hours) — mirror CreateJwtToken
        // - user:         minimal profile for client convenience
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

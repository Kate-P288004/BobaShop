// -----------------------------------------------------------------------------
// File: Controllers/AccountController.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   MVC controller that handles user registration, login, logout, and the
//   "access denied" page for the BoBaTastic site.
//
//   Storage/Auth stack:
//     - ASP.NET Core Identity (ApplicationUser) backed by SQLite per Program.cs
//     - Cookie authentication for the Web app (separate from API JWTs)
//     - Identity roles used by [Authorize(Policy = "RequireAdmin")] in Admin area
//
//   Key flows this controller covers:
//     - Register: create ApplicationUser, add "Customer" role, sign in
//     - Login: validate credentials, respect RememberMe, safe ReturnUrl redirect
//     - Logout: sign out and clear auth cookie
//     - AccessDenied: show a friendly page when role/auth fails
//
// Notes and pitfalls:
//   - Email is used as both username and identifier to simplify login UX.
//   - EmailConfirmed is set true for demo simplicity. For production, send a
//     confirmation email and require confirmation before allowing login.
//   - lockoutOnFailure is enabled on login to support Identity lockout policy.
//   - ReturnUrl is only honored if it’s a local URL (anti-open-redirect).
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Data;    // Contains the custom ApplicationUser model

namespace BobaShop.Web.Controllers
{
    // -------------------------------------------------------------------------
    // Controller: AccountController
    // Purpose:
    //   Entry point for account lifecycle in the Web app:
    //     • GET/POST Register
    //     • GET/POST Login
    //     • POST Logout
    //     • GET AccessDenied
    //
    // Dependencies:
    //   - UserManager<ApplicationUser>: user CRUD, roles, password hashing
    //   - SignInManager<ApplicationUser>: sign-in cookie, lockouts, 2FA hooks
    //
    // Rubric mapping hints:
    //   - ICTPRG556: routing, model binding, validation attributes, auth logic
    // -------------------------------------------------------------------------
    public class AccountController : Controller
    {
        // Identity managers: injected via DI in Program.cs
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        // ---------------------------------------------------------------------
        // Constructor
        // Wires up the Identity services that handle secure user and session ops.
        // ---------------------------------------------------------------------
        public AccountController(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        // =====================================================================
        // REGISTER
        // =====================================================================

        // ---------------------------------------------------------------------
        // GET: /Account/Register
        // Shows a blank registration form.
        // Anonymous access by design.
        // ---------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterVm());

        // ---------------------------------------------------------------------
        // POST: /Account/Register
        // Validates input, creates an ApplicationUser, assigns the "Customer"
        // role, and signs the user in. On success, return to Home.
        //
        // Validation path:
        //   - Data annotations on RegisterVm
        //   - Identity password policy enforced by UserManager.CreateAsync
        //
        // Error path:
        //   - If Identity fails (weak password, duplicate email), push errors
        //     into ModelState so the view can display them under the form.
        // ---------------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Build domain user object.
            // RewardPoints kept in ApplicationUser to support loyalty features.
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,   // Demo shortcut; in production send email
                RewardPoints = 0
            };

            // Persist user with hashed password.
            var result = await _users.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Default role aligns with API/AuthController assumptions.
                await _users.AddToRoleAsync(user, "Customer");

                // Establish the auth cookie for the Web app.
                await _signIn.SignInAsync(user, isPersistent: false);

                // Post-registration landing page. Could redirect to /Account/Profile later.
                return RedirectToAction("Index", "Home");
            }

            // Show Identity errors (e.g., password strength, duplicate email).
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        // =====================================================================
        // LOGIN
        // =====================================================================

        // ---------------------------------------------------------------------
        // GET: /Account/Login
        // Accepts an optional ReturnUrl; used after hitting an [Authorize] page.
        // The view should render the hidden ReturnUrl field so it round-trips.
        // ---------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVm { ReturnUrl = returnUrl });

        // ---------------------------------------------------------------------
        // POST: /Account/Login
        // Checks credentials and issues the auth cookie.
        //
        // Security notes:
        //   - lockoutOnFailure: true -> uses Identity lockout config for brute
        //     force protection. Configure lockout thresholds in Program.cs.
        //   - Returns a single generic error to avoid username enumeration.
        //   - Only redirects to ReturnUrl if it’s local (Url.IsLocalUrl).
        // ---------------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signIn.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Avoid detailed errors to prevent leaking validity of accounts.
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(vm);
        }

        // =====================================================================
        // LOGOUT + ACCESS DENIED
        // =====================================================================

        // ---------------------------------------------------------------------
        // POST: /Account/Logout
        // Invalidates the auth cookie and returns to the home page.
        // Anti-forgery token protects against CSRF on logout requests.
        // ---------------------------------------------------------------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------------------------------------------------------------------
        // GET: /Account/AccessDenied
        // Shown when authorization fails (e.g., missing Admin role).
        // The view should explain how to reach support or request access.
        // ---------------------------------------------------------------------
        public IActionResult AccessDenied() => View();

        // =====================================================================
        // VIEW MODELS
        // =====================================================================

        // ---------------------------------------------------------------------
        // RegisterVm
        // Bound to the registration form.
        // Fields:
        //   - Email: required, must be a valid email format
        //   - Password: required, password input
        //   - ConfirmPassword: must match Password
        //
        // Extend later:
        //   - DisplayName or FullName
        //   - Marketing opt-in
        //   - Terms acceptance checkbox
        // ---------------------------------------------------------------------
        public class RegisterVm
        {
            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required, DataType(DataType.Password), Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = "";
        }

        // ---------------------------------------------------------------------
        // LoginVm
        // Bound to the login form.
        // Fields:
        //   - Email/Password: standard credentials
        //   - RememberMe: persistent cookie if true
        //   - ReturnUrl: preserved across GET/POST to redirect back safely
        // ---------------------------------------------------------------------
        public class LoginVm
        {
            [Required, EmailAddress]
            public string Email { get; set; } = "";

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = "";

            public bool RememberMe { get; set; }
            public string? ReturnUrl { get; set; }
        }
    }
}

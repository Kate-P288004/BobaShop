// -----------------------------------------------------------------------------
// File: Controllers/AccountController.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Manages all account-related functionality for the BoBaTastic web interface.
//   Provides endpoints for user registration, login, logout, and access denied.
//   Uses ASP.NET Core Identity with a custom ApplicationUser model to store
//   customer details (including RewardPoints). Integrates with Identity roles
//   to distinguish Admin and Customer access within the system.
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
    //   Handles registration, authentication, and session management.
    //   Uses ASP.NET Identity dependency injection to securely manage users.
    //   This controller supports:
    //     • Registration (POST /Account/Register)
    //     • Login (POST /Account/Login)
    //     • Logout (POST /Account/Logout)
    //     • AccessDenied (GET /Account/AccessDenied)
    // Mapping: ICTPRG556 – MVC routing, model binding & auth logic
    // -------------------------------------------------------------------------
    public class AccountController : Controller
    {
        // ASP.NET Core Identity managers for user and sign-in operations
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        // ---------------------------------------------------------------------
        // Constructor:
        //   Injects UserManager and SignInManager services from Identity.
        //   These services provide APIs to create users, check passwords,
        //   manage roles, and maintain authentication cookies.
        // ---------------------------------------------------------------------
        public AccountController(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        // =====================================================================
        // REGISTER ACTIONS
        // =====================================================================

        // ---------------------------------------------------------------------
        // GET: /Account/Register
        // Purpose:
        //   Displays the registration form for new users.
        //   Accessible without authentication.
        // ---------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterVm());

        // ---------------------------------------------------------------------
        // POST: /Account/Register
        // Purpose:
        //   Handles form submission to create a new ApplicationUser.
        //   Automatically assigns the “Customer” role and logs them in.
        //   Uses data annotations to validate user input.
        // ---------------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Create the custom ApplicationUser instance
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,   // Simplified for demo
                RewardPoints = 0         // Start new users with zero points
            };

            // Attempt to create the user account in the Identity store
            var result = await _users.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Assign role and automatically sign in
                await _users.AddToRoleAsync(user, "Customer");
                await _signIn.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // If registration fails, display validation messages
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        // =====================================================================
        // LOGIN ACTIONS
        // =====================================================================

        // ---------------------------------------------------------------------
        // GET: /Account/Login
        // Purpose:
        //   Displays the login page. Includes an optional ReturnUrl to redirect
        //   back to a protected page after successful login.
        // ---------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVm { ReturnUrl = returnUrl });

        // ---------------------------------------------------------------------
        // POST: /Account/Login
        // Purpose:
        //   Validates user credentials using ASP.NET Identity’s sign-in manager.
        //   Supports “Remember Me” persistent login and local ReturnUrl redirects.
        // ---------------------------------------------------------------------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Attempt to sign in using email as the username
            var result = await _signIn.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Redirect to ReturnUrl if provided and valid, otherwise go home
                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Add generic error message for invalid credentials
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(vm);
        }

        // =====================================================================
        // LOGOUT + ACCESS DENIED
        // =====================================================================

        // ---------------------------------------------------------------------
        // POST: /Account/Logout
        // Purpose:
        //   Signs the current user out and clears the authentication cookie.
        //   Redirects to the Home page.
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
        // Purpose:
        //   Shown when a user tries to access a restricted page without the
        //   proper role or authorization level.
        // ---------------------------------------------------------------------
        public IActionResult AccessDenied() => View();

        // =====================================================================
        // VIEW MODELS (Inner Classes)
        // Purpose:
        //   Lightweight data containers used to bind form input values.
        //   Each ViewModel includes data annotations for server-side validation.
        // =====================================================================

        // ---------------------------------------------------------------------
        // RegisterVm: Used on the registration page
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
        // LoginVm: Used on the login page
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

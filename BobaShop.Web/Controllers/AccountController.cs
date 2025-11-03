// ---------------------------------------------------------------
// File: Controllers/AccountController.cs
// Student: Kate Odabas (P288004)
// Project: BoBaTastic – Web
// Purpose: Registration, Login, Logout, AccessDenied
// ---------------------------------------------------------------
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BobaShop.Web.Data;    // 👈 Our custom ApplicationUser lives here

namespace BobaShop.Web.Controllers
{
    public class AccountController : Controller
    {
        //  Use ApplicationUser, not IdentityUser
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public AccountController(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        // ---------- Register ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterVm());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // 👇 Create your custom user type
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,   // keep simple
                RewardPoints = 0         // start at zero
                // FullName = vm.FullName // add this if you later extend the VM
            };

            var result = await _users.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Default everyone to Customer
                await _users.AddToRoleAsync(user, "Customer");
                await _signIn.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        // ---------- Login ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVm { ReturnUrl = returnUrl });

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Using email as username, since we set UserName = Email at registration
            var result = await _signIn.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(vm);
        }

        // ---------- Logout ----------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();

        // ---------- ViewModels ----------
        public class RegisterVm
        {
            [Required, EmailAddress] public string Email { get; set; } = "";
            [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
            [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
            // public string? FullName { get; set; }   // uncomment if you want to collect
        }

        public class LoginVm
        {
            [Required, EmailAddress] public string Email { get; set; } = "";
            [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
            public bool RememberMe { get; set; }
            public string? ReturnUrl { get; set; }
        }
    }
}

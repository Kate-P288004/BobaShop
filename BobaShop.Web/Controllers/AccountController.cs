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

namespace BobaShop.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly SignInManager<IdentityUser> _signIn;

        public AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        // ---------- Register ----------
        [HttpGet]
        public IActionResult Register() => View(new RegisterVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new IdentityUser { UserName = vm.Email, Email = vm.Email, EmailConfirmed = true };
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
        public IActionResult Login(string? returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signIn.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);
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

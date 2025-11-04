// -----------------------------------------------------------------------------
// File: Controllers/AdminController.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Provides administrative access to system data and status reports.
//   Displays summary metrics such as API connectivity, drinks count,
//   logged-in admin details, and environment configuration.
//   Uses ASP.NET Core Identity for role-based access control and
//   calls the backend API through a typed service (IDrinksApi).
// -----------------------------------------------------------------------------

using BobaShop.Web.Data;
using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    // -------------------------------------------------------------------------
    // Controller: AdminController
    // Purpose:
    //   Restricts access to users assigned to the "Admin" role.
    //   Aggregates backend system data for the Admin Dashboard view.
    //   Demonstrates secure role-based access in an MVC environment.
    // Mapping: ICTPRG556  MVC routing, roles, and dependency injection
    // -------------------------------------------------------------------------
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // ASP.NET Identity manager for user account queries
        private readonly UserManager<ApplicationUser> _users;

        // Service layer abstraction for API communication (Drinks endpoints)
        private readonly IDrinksApi _drinksApi;

        // Application configuration and environment information
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        // ---------------------------------------------------------------------
        // Constructor:
        //   Injects Identity user manager, API service, and environment info.
        //   Enables separation of concerns by delegating API access to IDrinksApi.
        // ---------------------------------------------------------------------
        public AdminController(
            UserManager<ApplicationUser> users,
            IDrinksApi drinksApi,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            _users = users;
            _drinksApi = drinksApi;
            _config = config;
            _env = env;
        }

        // =====================================================================
        // GET: /Admin/Index
        // Purpose:
        //   Loads the Admin Dashboard page.
        //   Displays current user profile, system configuration,
        //   backend API connection status, and data counts.
        // Workflow:
        //   1. Get current admin user and their roles.
        //   2. Ping API via IDrinksApi.GetAllAsync().
        //   3. Return data to the AdminDashboardVm for rendering.
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // -----------------------------------------------------------------
            // 1. Retrieve current user and role information
            // -----------------------------------------------------------------
            var user = await _users.GetUserAsync(User);
            var roles = (await _users.GetRolesAsync(user)).ToList();

            // -----------------------------------------------------------------
            // 2. Collect API connectivity data
            // -----------------------------------------------------------------
            var apiBase = _config["Api:BaseUrl"] ?? "(not set)";
            bool apiOk = false;
            int drinksCount = 0;

            try
            {
                // Attempt to call the Drinks API service
                var drinks = await _drinksApi.GetAllAsync();
                drinksCount = drinks.Count;
                apiOk = true;
            }
            catch
            {
                // API may be offline, unreachable, or misconfigured
                apiOk = false;
            }

            // -----------------------------------------------------------------
            // 3. Prepare ViewModel for the dashboard view
            // -----------------------------------------------------------------
            var vm = new AdminDashboardVm
            {
                // User and account information
                Email = user?.Email ?? "(unknown)",
                UserId = user?.Id ?? "",
                Roles = roles,
                RewardPoints = user?.RewardPoints ?? 0,

                // System and environment details
                Environment = _env.EnvironmentName,
                AppVersion = _config["App:Version"] ?? "1.0.0",
                ServerTime = DateTime.Now,

                // Backend API status
                ApiBaseUrl = apiBase,
                ApiReachable = apiOk,
                DrinksCount = drinksCount
            };

            // -----------------------------------------------------------------
            // 4. Render the Admin Dashboard view with system summary data
            // -----------------------------------------------------------------
            return View(vm);
        }
    }
}

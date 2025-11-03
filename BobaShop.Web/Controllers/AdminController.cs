using BobaShop.Web.Data;
using BobaShop.Web.Models;
using BobaShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IDrinksApi _drinksApi;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);
            var roles = (await _users.GetRolesAsync(user)).ToList();

            // API status & counts
            var apiBase = _config["Api:BaseUrl"] ?? "(not set)";
            bool apiOk = false;
            int drinksCount = 0;
            try
            {
                var drinks = await _drinksApi.GetAllAsync();
                drinksCount = drinks.Count;
                apiOk = true;
            }
            catch
            {
                apiOk = false;
            }

            var vm = new AdminDashboardVm
            {
                Email = user?.Email ?? "(unknown)",
                UserId = user?.Id ?? "",
                Roles = roles,
                RewardPoints = user?.RewardPoints ?? 0,
                Environment = _env.EnvironmentName,
                AppVersion = _config["App:Version"] ?? "1.0.0",
                ServerTime = DateTime.Now,
                ApiBaseUrl = apiBase,
                ApiReachable = apiOk,
                DrinksCount = drinksCount
            };

            return View(vm);
        }
    }
}

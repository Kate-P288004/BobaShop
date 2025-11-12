using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdmin")]
    public class CartController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
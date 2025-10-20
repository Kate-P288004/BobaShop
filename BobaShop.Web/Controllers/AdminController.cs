// ---------------------------------------------------------------
// File: Controllers/AdminController.cs
// Student: Kate Odabas (P288004)
// Purpose: Simple Admin-only page for verification/screenshots
// ---------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index() => View();
    }
}

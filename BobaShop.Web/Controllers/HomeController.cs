// -----------------------------------------------------------------------------
// File: Controllers/HomeController.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Serves as the main entry point controller for the BoBaTastic web application.
//   Handles basic page navigation (Home, Privacy, Error) and demonstrates
//   ASP.NET Core MVC fundamentals such as routing, logging, and response caching.
//   Provides a simple Error action that integrates with the global exception
//   handler for consistent user-friendly feedback.
// -----------------------------------------------------------------------------

using BobaShop.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BobaShop.Web.Controllers
{
    // -------------------------------------------------------------------------
    // Controller: HomeController
    // Purpose:
    //   Provides the default routes and views for the application’s landing pages.
    //   This controller represents the simplest form of an MVC controller and is
    //   scaffolded by default in ASP.NET Core projects. It remains useful for
    //   project startup validation, route testing, and privacy policy display.
    // Mapping: ICTPRG556 MVC routing, controller/view integration
    // -------------------------------------------------------------------------
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // ---------------------------------------------------------------------
        // Constructor:
        //   Injects an ILogger instance to record diagnostic information such as
        //   user requests, page load errors, or unexpected behaviour.
        // ---------------------------------------------------------------------
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // =====================================================================
        // GET: /Home/Index
        // Purpose:
        //   Displays the main landing page of the BoBaTastic site.
        //   The corresponding Razor View (Views/Home/Index.cshtml) presents
        //   featured products, navigation links, and promotional content.
        // =====================================================================
        public IActionResult Index()
        {
            _logger.LogInformation("Home page loaded successfully.");
            return View();
        }

        // =====================================================================
        // GET: /Home/Privacy
        // Purpose:
        //   Displays the privacy policy page for compliance with legal and
        //   organisational data-handling requirements.
        //   The page content can be updated in Views/Home/Privacy.cshtml.
        // =====================================================================
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page viewed.");
            return View();
        }

        // =====================================================================
        // GET: /Home/Error
        // Purpose:
        //   Handles unexpected application errors.
        //   Creates an ErrorViewModel with the current request ID for debugging.
        //   The ResponseCache attribute ensures no client-side caching so that
        //   error messages always reflect current server state.
        // =====================================================================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error encountered on request {RequestId}", requestId);

            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }
    }
}

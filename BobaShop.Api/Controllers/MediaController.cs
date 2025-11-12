// -----------------------------------------------------------------------------
// File: Controllers/MediaController.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Provides static preset image listings for UI selection, such as default
//   product thumbnails. This controller currently supports "drinks" presets,
//   but can be extended to other media categories (e.g., toppings, banners).
// Notes:
//   - Does not require authentication.
//   - Returns static paths served from wwwroot/images directory.
// -----------------------------------------------------------------------------

using Asp.Versioning;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Api.Controllers
{
    // -------------------------------------------------------------------------
    // Controller: MediaController
    // Versioned API controller for serving static media presets.
    //
    // Example routes:
    //   GET /api/v1/media/presets/drinks
    //
    // Purpose:
    //   Provides a list of available preset images for use in front-end editors
    //   or product creation pages, allowing admins to choose from predefined
    //   images without manual upload.
    //
    // Authentication:
    //   - All endpoints are [AllowAnonymous] — anyone can access.
    //   - No database access is required; the data is built in-memory.
    //
    // Future extension:
    //   - Could dynamically read from configuration (MediaSettings)
    //     or file scanning to auto-discover available image assets.
    // -------------------------------------------------------------------------
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MediaController : ControllerBase
    {
        // ---------------------------------------------------------------------
        // Endpoint: GET /api/v1/media/presets/{category}
        // Description:
        //   Returns a predefined list of image URLs and alt text strings.
        //   Useful for admin UIs to display image pickers.
        //
        // Parameters:
        //   category - string (path variable)
        //              Expected values: "drinks"
        //
        // Behavior:
        //   - If the requested category matches "drinks" (case-insensitive),
        //     returns a list of PresetImageDto objects.
        //   - If not recognized, returns HTTP 404.
        //
        // Response:
        //   - 200 OK with IEnumerable<PresetImageDto>
        //   - 404 NotFound if unsupported category
        // ---------------------------------------------------------------------
        [HttpGet("presets/{category}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PresetImageDto>> GetPresets(string category)
        {
            // Verify that the requested category is supported.
            // Currently only "drinks" presets are defined.
            if (!string.Equals(category, "drinks", StringComparison.OrdinalIgnoreCase))
                return NotFound();

            // Return a static list of available images from /wwwroot/images/drinks/.
            // Each item defines:
            //   - Path relative to the site root (served by UseStaticFiles)
            //   - Alt text for accessibility and SEO
            var list = new List<PresetImageDto>
            {
                new("/images/drinks/1.jpg", "Drink 1"),
                new("/images/drinks/2.jpg", "Drink 2"),
                new("/images/drinks/3.jpg", "Drink 3"),
                new("/images/drinks/4.jpg", "Drink 4"),
                new("/images/drinks/5.jpg", "Drink 5"),
                new("/images/drinks/6.jpg", "Drink 6"),
                new("/images/drinks/7.jpg", "Drink 7"),
                new("/images/drinks/8.jpg", "Drink 8"),
                new("/images/drinks/9.jpg", "Drink 9"),
                new("/images/drinks/10.jpg", "Drink 10")
            };

            // Return 200 OK with the image list as JSON.
            return Ok(list);
        }
    }
}

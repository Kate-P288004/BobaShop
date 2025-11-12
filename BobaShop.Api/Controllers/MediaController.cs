using Asp.Versioning;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MediaController : ControllerBase
    {
        // Anyone can read presets (no token required)
        [HttpGet("presets/{category}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PresetImageDto>> GetPresets(string category)
        {
            // For now we only serve drinks; extend if needed
            if (!string.Equals(category, "drinks", StringComparison.OrdinalIgnoreCase))
                return NotFound();

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

            return Ok(list);
        }
    }
}

// ---------------------------------------------------------------
// File: DrinksV2Controller.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
//   API v2 controller for assessment evidence of versioning.
//   - Demonstrates versioned routes: /api/v2/drinks
//   - Read-only endpoints with wrapped metadata
//   - Uses Asp.Versioning.ApiVersion("2.0")
// ---------------------------------------------------------------

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BobaShop.Api.Data;
using BobaShop.Api.Models;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Asp.Versioning.ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DrinksV2Controller : ControllerBase
    {
        private readonly MongoDbContext _ctx;

        public DrinksV2Controller(MongoDbContext ctx)
        {
            _ctx = ctx;
        }

        // -----------------------------------------------------------
        // GET: /api/v2/drinks
        // Returns all active drinks (soft-deleted excluded)
        // Adds apiVersion and count metadata for demonstration.
        // -----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _ctx.Drinks
                .Find(d => d.IsActive && d.DeletedUtc == null)
                .SortBy(d => d.Name)
                .ToListAsync();

            return Ok(new
            {
                apiVersion = "2.0",
                count = items.Count,
                items
            });
        }

        // -----------------------------------------------------------
        // GET: /api/v2/drinks/{id}
        // Retrieve a specific drink by id (wrapped response)
        // -----------------------------------------------------------
        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetById(string id)
        {
            var drink = await _ctx.Drinks
                .Find(d => d.Id == id && d.DeletedUtc == null)
                .FirstOrDefaultAsync();

            if (drink is null)
                return NotFound(new
                {
                    apiVersion = "2.0",
                    message = "Drink not found"
                });

            return Ok(new
            {
                apiVersion = "2.0",
                drink
            });
        }
    }
}

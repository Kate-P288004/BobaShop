// -----------------------------------------------------------------------------
// File: Controllers/DrinksController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Description:
//   CRUD for Drinks in MongoDB.
//   - GETs are public
//   - POST/PUT/DELETE require Admin (JWT bearer)
//   - Soft delete via DeletedUtc
//   - Maps ImageUrl/ImageAlt
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BobaShop.Api.Data;
using BobaShop.Api.Dtos;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DrinksController : ControllerBase
    {
        private readonly MongoDbContext _ctx;

        public DrinksController(MongoDbContext ctx)
        {
            // Store injected MongoDbContext; throw early if DI is misconfigured.
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        // GET: api/v1/Drinks
        // Purpose:
        //   Returns all drinks, ordered by Name.
        //   By default excludes soft-deleted documents (DeletedUtc != null).
        // Query:
        //   includeDeleted=true -> returns all documents including soft-deleted ones.
        // Response:
        //   200 OK with array of Drink documents.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Drink>>> GetAll([FromQuery] bool includeDeleted = false)
        {
            // Build a conditional filter based on includeDeleted flag.
            // - When false (default): only active items (DeletedUtc == null).
            // - When true: no filter (returns everything).
            var filter = includeDeleted
                ? Builders<Drink>.Filter.Empty
                : Builders<Drink>.Filter.Eq(d => d.DeletedUtc, null);

            // Query and sort by Name for a stable, user-friendly listing.
            var list = await _ctx.Drinks
                .Find(filter)
                .SortBy(d => d.Name)
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/v1/Drinks/{id}
        // Purpose:
        //   Retrieves a single drink by its MongoDB ObjectId (24-char hex).
        // Validation:
        //   - Returns 400 if the id is not a valid ObjectId format.
        //   - Returns 404 if no document found.
        [HttpGet("{id}")]
        public async Task<ActionResult<Drink>> GetById(string id)
        {
            // Validate the id format early to avoid a server-side cast error.
            if (!ObjectId.TryParse(id, out _))
                return BadRequest(new { error = "Invalid id format" });

            // Find the document by Id; include soft-deleted if it matches the id.
            var drink = await _ctx.Drinks
                .Find(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (drink is null) return NotFound();
            return Ok(drink);
        }

        // POST: api/v1/Drinks   (Admin only)
        // Security:
        //   Requires a valid JWT with role "Admin".
        // Purpose:
        //   Creates a new Drink document from a validated DTO.
        // Behavior:
        //   - Sets Price = BasePrice initially (effective price equals base).
        //   - Trims strings; null -> empty for Description, null for images.
        // Responses:
        //   - 201 Created with the created document and Location header.
        //   - 400 ValidationProblem if data annotations fail.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DrinkCreateDto dto)
        {
            // Model binding + data annotations produce ModelState errors automatically.
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Map DTO -> domain model; maintain a predictable defaulting strategy.
            var drink = new Drink
            {
                Name = dto.Name?.Trim() ?? string.Empty,
                Description = dto.Description?.Trim() ?? string.Empty,
                BasePrice = dto.BasePrice,        // zero allowed
                Price = dto.BasePrice,        // persist effective price = base
                SmallUpcharge = dto.SmallUpcharge,
                MediumUpcharge = dto.MediumUpcharge,
                LargeUpcharge = dto.LargeUpcharge,
                DefaultSugar = dto.DefaultSugar,
                DefaultIce = dto.DefaultIce,
                IsActive = dto.IsActive,
                ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl,
                ImageAlt = string.IsNullOrWhiteSpace(dto.ImageAlt) ? null : dto.ImageAlt
            };

            // Insert the new document.
            await _ctx.Drinks.InsertOneAsync(drink);

            // Return a 201 pointing back to the GET by id route.
            return CreatedAtAction(nameof(GetById), new { id = drink.Id }, drink);
        }

        // PUT: api/v1/Drinks/{id}   (Admin only)
        // Security:
        //   Requires "Admin" role.
        // Purpose:
        //   Full update of a Drink document; fields are replaced with DTO values.
        // Validation:
        //   - Ensures route id is a valid ObjectId.
        //   - Validates DTO via ModelState; trims strings.
        // Behavior:
        //   - Price stays aligned to BasePrice.
        //   - UpdatedUtc is set to current UTC.
        // Responses:
        //   - 204 NoContent on success.
        //   - 404 if the target document does not exist.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DrinkUpdateDto dto)
        {
            // Defensive check: id must be a valid ObjectId string.
            if (!ObjectId.TryParse(id, out _))
                return BadRequest(new { error = "Invalid id format" });

            // Force body Id to match the route for traceability on the server side.
            dto.Id = id;

            // Reject if data annotations fail.
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var filter = Builders<Drink>.Filter.Eq(d => d.Id, id);

            // Fetch the current document so we can preserve fields as needed.
            var existed = await _ctx.Drinks.Find(filter).FirstOrDefaultAsync();
            if (existed is null) return NotFound();

            // Apply updates (normalize strings, recalc effective price).
            existed.Name = dto.Name.Trim();
            existed.Description = dto.Description?.Trim() ?? string.Empty;
            existed.BasePrice = dto.BasePrice;
            existed.Price = dto.BasePrice;     // zero allowed
            existed.SmallUpcharge = dto.SmallUpcharge;
            existed.MediumUpcharge = dto.MediumUpcharge;
            existed.LargeUpcharge = dto.LargeUpcharge;
            existed.DefaultSugar = dto.DefaultSugar;
            existed.DefaultIce = dto.DefaultIce;
            existed.IsActive = dto.IsActive;
            existed.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl;
            existed.ImageAlt = string.IsNullOrWhiteSpace(dto.ImageAlt) ? null : dto.ImageAlt;
            existed.UpdatedUtc = DateTime.UtcNow;

            // Replace in full to keep the document consistent.
            await _ctx.Drinks.ReplaceOneAsync(filter, existed);
            return NoContent();
        }


        // DELETE: api/v1/Drinks/{id}   (Admin only, soft delete)
        // Security:
        //   Requires "Admin" role.
        // Purpose:
        //   Performs a soft delete by setting DeletedUtc and disabling the item.
        // Behavior:
        //   - Idempotent: if already soft-deleted, returns 204 again.
        //   - Sets UpdatedUtc to help with audit/history queries.
        // Responses:
        //   - 204 NoContent on success.
        //   - 404 if the document does not exist.
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // Validate id early; prevents needless DB round-trips.
            if (!ObjectId.TryParse(id, out _))
                return BadRequest(new { error = "Invalid id format" });

            var filter = Builders<Drink>.Filter.Eq(d => d.Id, id);
            var drink = await _ctx.Drinks.Find(filter).FirstOrDefaultAsync();
            if (drink is null) return NotFound();

            // If already deleted, treat as success (idempotent API design).
            if (drink.DeletedUtc is not null)
                return NoContent(); // idempotent

            // Soft-delete + deactivate + set UpdatedUtc for traceability.
            drink.IsActive = false;
            drink.DeletedUtc = DateTime.UtcNow;
            drink.UpdatedUtc = DateTime.UtcNow;

            await _ctx.Drinks.ReplaceOneAsync(filter, drink);
            return NoContent();
        }
    }
}

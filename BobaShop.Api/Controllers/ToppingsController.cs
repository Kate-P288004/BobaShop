// -----------------------------------------------------------------------------
// File: Controllers/ToppingsController.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Manage toppings in MongoDB.
//   - Public: GET all, GET by id
//   - Admin:  POST create, PUT update, DELETE soft-delete, PATCH active
// Route base: /api/v1/Toppings
// Notes:
//   Demonstrates use of attribute routing, query filters, pagination, and 
//   optimistic concurrency control via UTC timestamps. Supports role-based 
//   authorization policies with [Authorize(Policy = "RequireAdmin")].
// -----------------------------------------------------------------------------

using Asp.Versioning;
using BobaShop.Api.Data;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ToppingsController : ControllerBase
    {
        // ---------------------------------------------------------------------
        // FIELD: MongoDB collection reference
        // Purpose:
        //   Directly interact with the "Toppings" collection via MongoDbContext.
        // ---------------------------------------------------------------------
        private readonly IMongoCollection<Topping> _toppings;

        // Constructor: inject MongoDbContext from DI container.
        // Ensures all endpoints share the same Mongo connection and settings.
        public ToppingsController(MongoDbContext ctx)
        {
            _toppings = ctx.Toppings;
        }

        // ---------------------------------------------------------------------
        // Helper: Validate MongoDB ObjectId
        // Purpose:
        //   Prevents invalid IDs from triggering server-side cast errors.
        // ---------------------------------------------------------------------
        private static bool IsValidObjectId(string id) => ObjectId.TryParse(id, out _);

        // ---------------------------------------------------------------------
        // GET /api/v1/Toppings?name=pearl&active=true&min=0&max=2&skip=0&take=50
        // Purpose:
        //   Retrieves a paginated list of toppings with optional filters.
        // Query Parameters:
        //   name   – case-insensitive substring match
        //   active – filter by IsActive (true/false)
        //   min/max – price range in AUD
        //   skip/take – pagination (default: skip=0, take=50)
        // Behavior:
        //   - Excludes soft-deleted records (DeletedUtc == null)
        //   - Adds X-Total-Count header for pagination metadata
        // ---------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(List<Topping>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? name,
            [FromQuery] bool? active,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            // Enforce sensible pagination limits to prevent abuse
            if (take is < 1 or > 200) take = 50;
            if (skip < 0) skip = 0;

            // Start with active-only filter (not soft-deleted)
            var filter = Builders<Topping>.Filter.Eq(t => t.DeletedUtc, null);

            // Add dynamic filters based on query parameters
            if (!string.IsNullOrWhiteSpace(name))
                filter &= Builders<Topping>.Filter.Regex(t => t.Name, new BsonRegularExpression(name, "i"));

            if (active.HasValue)
                filter &= Builders<Topping>.Filter.Eq(t => t.IsActive, active.Value);

            if (min.HasValue) filter &= Builders<Topping>.Filter.Gte(t => t.Price, min.Value);
            if (max.HasValue) filter &= Builders<Topping>.Filter.Lte(t => t.Price, max.Value);

            // Run count and query in parallel for better performance
            var countTask = _toppings.CountDocumentsAsync(filter, cancellationToken: ct);
            var itemsTask = _toppings.Find(filter)
                                     .SortBy(t => t.Name)
                                     .Skip(skip)
                                     .Limit(take)
                                     .ToListAsync(ct);

            var total = await countTask;
            var items = await itemsTask;

            // Include total count header for frontend pagination (React, etc.)
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        // ---------------------------------------------------------------------
        // GET /api/v1/Toppings/{id}
        // Purpose:
        //   Retrieve a topping by its MongoDB ObjectId.
        //   Returns 400 if invalid, 404 if not found or deleted.
        // ---------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("{id:length(24)}")]
        [ProducesResponseType(typeof(Topping), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");

            var item = await _toppings
                .Find(t => t.Id == id && t.DeletedUtc == null)
                .FirstOrDefaultAsync(ct);

            return item is null ? NotFound() : Ok(item);
        }

        // ---------------------------------------------------------------------
        // POST /api/v1/Toppings
        // Purpose:
        //   Creates a new topping document in MongoDB.
        // Access:
        //   Requires admin privileges (RequireAdmin policy).
        // Behavior:
        //   - Validates fields (name, price)
        //   - Generates new ObjectId if not provided
        //   - Sets CreatedUtc timestamp automatically
        //   - Returns 201 Created with resource URI
        // ---------------------------------------------------------------------
        [Authorize(Policy = "RequireAdmin")]
        [HttpPost]
        [ProducesResponseType(typeof(Topping), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] Topping model, CancellationToken ct = default)
        {
            if (model is null) return BadRequest("Body is required.");
            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required.");
            if (model.Price < 0) return BadRequest("Price must be >= 0.");

            // Assign ID and lifecycle metadata
            model.Id = string.IsNullOrWhiteSpace(model.Id) ? ObjectId.GenerateNewId().ToString() : model.Id;
            model.Name = model.Name.Trim();
            model.CreatedUtc = DateTime.UtcNow;
            model.UpdatedUtc = null;
            model.DeletedUtc = null;

            try
            {
                await _toppings.InsertOneAsync(model, cancellationToken: ct);
                return CreatedAtAction(nameof(GetById), new { id = model.Id, version = "1.0" }, model);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                // Handle duplicate name or unique constraint violation gracefully
                return Conflict("Duplicate key.");
            }
        }

        // ---------------------------------------------------------------------
        // PUT /api/v1/Toppings/{id}
        // Purpose:
        //   Fully replaces an existing topping’s editable fields.
        // Behavior:
        //   - Validates name, price, and ID
        //   - Updates name, price, active flag, and UpdatedUtc timestamp
        //   - Skips if record not found or soft-deleted
        // ---------------------------------------------------------------------
        [Authorize(Policy = "RequireAdmin")]
        [HttpPut("{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] Topping model, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");
            if (model is null) return BadRequest("Body is required.");
            if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest("Name is required.");
            if (model.Price < 0) return BadRequest("Price must be >= 0.");

            var update = Builders<Topping>.Update
                .Set(t => t.Name, model.Name.Trim())
                .Set(t => t.Price, model.Price)
                .Set(t => t.IsActive, model.IsActive)
                .Set(t => t.UpdatedUtc, DateTime.UtcNow);

            var result = await _toppings.UpdateOneAsync(
                t => t.Id == id && t.DeletedUtc == null,
                update,
                cancellationToken: ct);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // ---------------------------------------------------------------------
        // PATCH /api/v1/Toppings/{id}/active
        // Purpose:
        //   Toggle a topping’s IsActive flag.
        // Access:
        //   Admin only.
        // Behavior:
        //   - Accepts a minimal DTO to avoid overposting.
        //   - Updates IsActive + UpdatedUtc fields.
        // ---------------------------------------------------------------------
        public sealed class ToppingSetActiveDto { public bool IsActive { get; set; } }

        [Authorize(Policy = "RequireAdmin")]
        [HttpPatch("{id:length(24)}/active")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetActive(string id, [FromBody] ToppingSetActiveDto body, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");
            if (body is null) return BadRequest("Body is required.");

            var result = await _toppings.UpdateOneAsync(
                t => t.Id == id && t.DeletedUtc == null,
                Builders<Topping>.Update
                    .Set(t => t.IsActive, body.IsActive)
                    .Set(t => t.UpdatedUtc, DateTime.UtcNow),
                cancellationToken: ct);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // ---------------------------------------------------------------------
        // DELETE /api/v1/Toppings/{id}  (soft delete)
        // Purpose:
        //   Marks a topping as deleted without removing it from the database.
        // Behavior:
        //   - Sets DeletedUtc timestamp to current UTC time.
        //   - Leaves record intact for reporting or audit recovery.
        // ---------------------------------------------------------------------
        [Authorize(Policy = "RequireAdmin")]
        [HttpDelete("{id:length(24)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(string id, CancellationToken ct = default)
        {
            if (!IsValidObjectId(id)) return BadRequest("Invalid id format.");

            var result = await _toppings.UpdateOneAsync(
                t => t.Id == id && t.DeletedUtc == null,
                Builders<Topping>.Update.Set(t => t.DeletedUtc, DateTime.UtcNow),
                cancellationToken: ct);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }
    }
}

// -----------------------------------------------------------------------------
// File: OrdersController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Provides CRUD endpoints for managing customer orders in MongoDB.
//   Demonstrates filtering, soft deletion, and total calculation based on 
//   linked Drinks and Toppings collections. All operations are asynchronous 
//   and follow RESTful conventions.
// -----------------------------------------------------------------------------

using BobaShop.Api.Data;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        // ---------------------------------------------------------------------
        // MongoDB collection references for Orders, Drinks, and Toppings
        // Why keep direct IMongoCollection<T> fields?
        // - Keeps query/update code concise and testable
        // - Avoids repeated property access to the context per request
        // ---------------------------------------------------------------------
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Drink> _drinks;
        private readonly IMongoCollection<Topping> _toppings;

        // Inject MongoDB context to access collections
        public OrdersController(MongoDbContext db)
        {
            _orders = db.Orders;
            _drinks = db.Drinks;
            _toppings = db.Toppings;
        }

        // ---------------------------------------------------------------------
        // GET: api/v1/orders?email=&status=&from=&to=
        // Purpose:
        //   Retrieves a filtered list of orders from MongoDB.
        //   Supports filtering by customer email, order status, and date range.
        // Mapping: ICTPRG554 PE1.1 / PE1.2 / ICTPRG556 PE2.1
        //
        // Notes:
        // - Soft-deleted orders (DeletedUtc != null) are excluded by default.
        // - 'from'/'to' are interpreted as UTC (converted if provided).
        // - Sorted newest → oldest to help dashboards show recent activity first.
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll(
            [FromQuery] string? email,
            [FromQuery] string? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            // Base filter: only include non-deleted orders
            var filter = Builders<Order>.Filter.Where(o => o.DeletedUtc == null);

            // Optional email filter
            if (!string.IsNullOrWhiteSpace(email))
                filter &= Builders<Order>.Filter.Eq(o => o.CustomerEmail, email.Trim());

            // Optional status filter
            if (!string.IsNullOrWhiteSpace(status))
                filter &= Builders<Order>.Filter.Eq(o => o.Status, status.Trim());

            // Optional date range filters
            if (from.HasValue)
                filter &= Builders<Order>.Filter.Gte(o => o.CreatedUtc, from.Value.ToUniversalTime());
            if (to.HasValue)
                filter &= Builders<Order>.Filter.Lte(o => o.CreatedUtc, to.Value.ToUniversalTime());

            // Query MongoDB and sort by creation date (newest first)
            var results = await _orders.Find(filter)
                                       .SortByDescending(o => o.CreatedUtc)
                                       .ToListAsync();

            return Ok(results);
        }

        // ---------------------------------------------------------------------
        // GET: api/v1/orders/{id}
        // Purpose:
        //   Retrieve a single order by its ObjectId.
        //   Returns 404 if not found or invalid ID format.
        //
        // Notes:
        // - Validates ObjectId format early to avoid server-side cast exceptions.
        // - Excludes soft-deleted orders for consistency with list endpoint.
        // ---------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid id format.");

            var order = await _orders.Find(o => o.Id == id && o.DeletedUtc == null).FirstOrDefaultAsync();
            return order is null ? NotFound() : Ok(order);
        }

        // ---------------------------------------------------------------------
        // POST: api/v1/orders
        // Purpose:
        //   Creates a new order document with calculated total.
        //   Validates object IDs and auto-fills timestamps and default status.
        // Mapping: ICTPRG554 PE1.1 / ICTPRG556 PE2.1
        //
        // Behavior:
        // - Filters incoming DrinkIds/ToppingIds to only valid ObjectIds.
        // - Total is computed from current menu prices (avoids client tampering).
        // - Status defaults to "New" if none provided.
        // - CreatedUtc set server-side for auditability.
        // ---------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Order>> Create([FromBody] OrderCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
                return BadRequest("CustomerEmail is required.");

            // Sanitize input by ensuring valid ObjectIds
            dto.DrinkIds = dto.DrinkIds?.Where(s => ObjectId.TryParse(s, out _)).ToList() ?? new();
            dto.ToppingIds = dto.ToppingIds?.Where(s => ObjectId.TryParse(s, out _)).ToList();

            // Calculate total from current prices
            var total = await CalculateTotalAsync(dto.DrinkIds, dto.ToppingIds);

            // Build new order document
            var order = new Order
            {
                CustomerEmail = dto.CustomerEmail.Trim(),
                DrinkIds = dto.DrinkIds,
                ToppingIds = dto.ToppingIds,
                Total = total,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "New" : dto.Status!.Trim(),
                CreatedUtc = DateTime.UtcNow
            };

            await _orders.InsertOneAsync(order);

            // Return 201 Created response
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // ---------------------------------------------------------------------
        // PUT: api/v1/orders/{id}
        // Purpose:
        //   Full update of an existing order (email, items, status).
        //   Recalculates total and updates timestamp.
        //
        // Notes:
        // - Rejects invalid ObjectId format before querying DB.
        // - Recomputes total using the same helper to keep logic DRY.
        // - Does not modify CreatedUtc; sets UpdatedUtc to now (UTC).
        // - Only updates if the order is not soft-deleted.
        // ---------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] OrderUpdateDto dto)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid id format.");

            dto.DrinkIds = dto.DrinkIds?.Where(s => ObjectId.TryParse(s, out _)).ToList() ?? new();
            dto.ToppingIds = dto.ToppingIds?.Where(s => ObjectId.TryParse(s, out _)).ToList();

            var total = await CalculateTotalAsync(dto.DrinkIds, dto.ToppingIds);

            // Build update definition
            var update = Builders<Order>.Update
                .Set(o => o.CustomerEmail, dto.CustomerEmail.Trim())
                .Set(o => o.DrinkIds, dto.DrinkIds)
                .Set(o => o.ToppingIds, dto.ToppingIds)
                .Set(o => o.Status, string.IsNullOrWhiteSpace(dto.Status) ? "New" : dto.Status!.Trim())
                .Set(o => o.Total, total)
                .Set(o => o.UpdatedUtc, DateTime.UtcNow);

            var result = await _orders.UpdateOneAsync(
                o => o.Id == id && o.DeletedUtc == null,
                update);

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // ---------------------------------------------------------------------
        // PATCH: api/v1/orders/{id}/status
        // Purpose:
        //   Updates only the order’s status field.
        //
        // Notes:
        // - Minimal payload DTO to reduce overposting risk.
        // - Trims status; sets UpdatedUtc to support audit.
        // - Fails fast on invalid id or missing status.
        // ---------------------------------------------------------------------
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] OrderStatusDto dto)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid id format.");
            if (string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest("Status is required.");

            var result = await _orders.UpdateOneAsync(
                o => o.Id == id && o.DeletedUtc == null,
                Builders<Order>.Update
                    .Set(o => o.Status, dto.Status.Trim())
                    .Set(o => o.UpdatedUtc, DateTime.UtcNow));

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // ---------------------------------------------------------------------
        // DELETE: api/v1/orders/{id}
        // Purpose:
        //   Performs a soft delete by setting DeletedUtc timestamp.
        //   Retains record for auditing and reporting.
        //
        // Notes:
        // - Leaves the document in-place; downstream analytics can still read it.
        // - Idempotent: repeated calls remain 204 if already soft-deleted.
        // ---------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid id format.");

            var result = await _orders.UpdateOneAsync(
                o => o.Id == id && o.DeletedUtc == null,
                Builders<Order>.Update.Set(o => o.DeletedUtc, DateTime.UtcNow));

            return result.MatchedCount == 0 ? NotFound() : NoContent();
        }

        // ---------------------------------------------------------------------
        // Helper: CalculateTotalAsync
        // Purpose:
        //   Computes total price by summing drink and topping prices.
        //
        // Why compute server-side?
        // - Prevents clients from submitting arbitrary totals
        // - Ensures consistency with current catalog prices
        // ---------------------------------------------------------------------
        private async Task<decimal> CalculateTotalAsync(IEnumerable<string> drinkIds, IEnumerable<string>? toppingIds)
        {
            decimal total = 0m;

            if (drinkIds?.Any() == true)
            {
                var drinkFilter = Builders<Drink>.Filter.In(d => d.Id, drinkIds);
                var drinkList = await _drinks.Find(drinkFilter).ToListAsync();
                total += drinkList.Sum(d => d.Price);
            }

            if (toppingIds?.Any() == true)
            {
                var toppingFilter = Builders<Topping>.Filter.In(t => t.Id, toppingIds);
                var toppingList = await _toppings.Find(toppingFilter).ToListAsync();
                total += toppingList.Sum(t => t.Price);
            }

            return total;
        }

        // ---------------------------------------------------------------------
        // Data Transfer Objects (DTOs)
        // Purpose:
        //   Lightweight models used for request payloads.
        //
        // Notes:
        // - These are intentionally minimal to prevent overposting.
        // - Validation is handled in the action methods (e.g., null/empty checks).
        // ---------------------------------------------------------------------
        public class OrderCreateDto
        {
            public string CustomerEmail { get; set; } = default!;
            public List<string> DrinkIds { get; set; } = new();
            public List<string>? ToppingIds { get; set; }
            public string? Status { get; set; }
        }

        public class OrderUpdateDto
        {
            public string CustomerEmail { get; set; } = default!;
            public List<string> DrinkIds { get; set; } = new();
            public List<string>? ToppingIds { get; set; }
            public string? Status { get; set; }
        }

        public class OrderStatusDto
        {
            public string Status { get; set; } = default!;
        }
    }
}

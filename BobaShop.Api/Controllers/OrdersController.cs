using BobaShop.Api.Data;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BobaShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMongoCollection<Order> _orders;
    private readonly IMongoCollection<Drink> _drinks;
    private readonly IMongoCollection<Topping> _toppings;

    public OrdersController(MongoDbContext db)
    {
        _orders = db.Orders;
        _drinks = db.Drinks;
        _toppings = db.Toppings;
    }

    // GET: api/v1/orders?email=&status=&from=2025-10-01&to=2025-10-31
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(
        [FromQuery] string? email,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var filter = Builders<Order>.Filter.Where(o => o.DeletedUtc == null);

        if (!string.IsNullOrWhiteSpace(email))
            filter &= Builders<Order>.Filter.Eq(o => o.CustomerEmail, email.Trim());

        if (!string.IsNullOrWhiteSpace(status))
            filter &= Builders<Order>.Filter.Eq(o => o.Status, status.Trim());

        if (from.HasValue)
            filter &= Builders<Order>.Filter.Gte(o => o.CreatedUtc, from.Value.ToUniversalTime());

        if (to.HasValue)
            filter &= Builders<Order>.Filter.Lte(o => o.CreatedUtc, to.Value.ToUniversalTime());

        var results = await _orders.Find(filter)
                                   .SortByDescending(o => o.CreatedUtc)
                                   .ToListAsync();

        return Ok(results);
    }

    // GET: api/v1/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

        var order = await _orders.Find(o => o.Id == id && o.DeletedUtc == null).FirstOrDefaultAsync();
        return order is null ? NotFound() : Ok(order);
    }

    // POST: api/v1/orders
    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] OrderCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
            return BadRequest("CustomerEmail is required.");

        // Sanitize ids
        // In Create()
        dto.DrinkIds = dto.DrinkIds?.Where(s => ObjectId.TryParse(s, out _)).ToList() ?? new();
        dto.ToppingIds = dto.ToppingIds?.Where(s => ObjectId.TryParse(s, out _)).ToList();

        // Calculate total from current prices
        var total = await CalculateTotalAsync(dto.DrinkIds, dto.ToppingIds);

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

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // PUT: api/v1/orders/{id}
    // Full update of drinks/toppings/status; total is recalculated
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] OrderUpdateDto dto)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

        dto.DrinkIds = dto.DrinkIds?.Where(s => ObjectId.TryParse(s, out _)).ToList() ?? new();
        dto.ToppingIds = dto.ToppingIds?.Where(s => ObjectId.TryParse(s, out _)).ToList();

        var total = await CalculateTotalAsync(dto.DrinkIds, dto.ToppingIds);

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

    // PATCH: api/v1/orders/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] OrderStatusDto dto)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");
        if (string.IsNullOrWhiteSpace(dto.Status)) return BadRequest("Status is required.");

        var result = await _orders.UpdateOneAsync(
            o => o.Id == id && o.DeletedUtc == null,
            Builders<Order>.Update
                .Set(o => o.Status, dto.Status.Trim())
                .Set(o => o.UpdatedUtc, DateTime.UtcNow));

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    // DELETE: api/v1/orders/{id}  (soft delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return BadRequest("Invalid id format.");

        var result = await _orders.UpdateOneAsync(
            o => o.Id == id && o.DeletedUtc == null,
            Builders<Order>.Update.Set(o => o.DeletedUtc, DateTime.UtcNow));

        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    // --- helpers ---

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

    // --- DTOs 

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

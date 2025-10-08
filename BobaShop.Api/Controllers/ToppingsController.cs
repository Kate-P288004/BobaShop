using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BobaShop.Api.Data;
using BobaShop.Api.Dtos;
using BobaShop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingsController : ControllerBase
    {
        private readonly MongoDbContext _db;
        public ToppingsController(MongoDbContext db) => _db = db;

        /// List toppings (hide soft-deleted unless includeDeleted=true)
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Topping>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            var filter = includeDeleted
                ? Builders<Topping>.Filter.Empty
                : Builders<Topping>.Filter.Eq(t => t.DeletedAt, null);

            var toppings = await _db.Toppings.Find(filter).SortBy(t => t.Name).ToListAsync();
            return Ok(toppings);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Topping), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return NotFound();
            var t = await _db.Toppings.Find(x => x.Id == id).FirstOrDefaultAsync();
            return t is null ? NotFound() : Ok(t);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Topping), 201)]
        public async Task<IActionResult> Create([FromBody] ToppingCreateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = new Topping
            {
                Name = dto.Name,
                Price = dto.Price,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Toppings.InsertOneAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(string id, [FromBody] ToppingUpdateDto dto)
        {
            if (!ObjectId.TryParse(id, out _)) return NotFound();

            var t = await _db.Toppings.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (t is null) return NotFound();

            t.Name = dto.Name;
            t.Price = dto.Price;
            t.IsActive = dto.IsActive;
            t.UpdatedAt = DateTime.UtcNow;

            var res = await _db.Toppings.ReplaceOneAsync(x => x.Id == id, t);
            return res.MatchedCount == 0 ? NotFound() : NoContent();
        }

        [HttpPatch("{id}/soft-delete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SoftDelete(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return NotFound();

            var update = Builders<Topping>.Update
                .Set(x => x.DeletedAt, DateTime.UtcNow)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var res = await _db.Toppings.UpdateOneAsync(x => x.Id == id && x.DeletedAt == null, update);
            return res.MatchedCount == 0 ? NotFound() : NoContent();
        }

        [HttpPatch("{id}/restore")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Restore(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return NotFound();

            var update = Builders<Topping>.Update
                .Set(x => x.DeletedAt, null)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var res = await _db.Toppings.UpdateOneAsync(x => x.Id == id && x.DeletedAt != null, update);
            return res.MatchedCount == 0 ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return NotFound();

            var res = await _db.Toppings.DeleteOneAsync(x => x.Id == id);
            return res.DeletedCount == 0 ? NotFound() : NoContent();
        }
    }
}

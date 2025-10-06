// -----------------------------------------------------------------------------
// File: DrinksController.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
// API controller providing CRUD endpoints for managing drinks in MongoDB.
// Demonstrates use of the Magic Three Dates for record lifecycle tracking.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using BobaShop.Api.Data;
using BobaShop.Api.Models;
using MongoDB.Driver;

namespace BobaShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrinksController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drinks;

        public DrinksController(MongoDbContext context)
        {
            _drinks = context.GetCollection<Drink>("Drinks");
        }

        // -------------------------------------------------------------
        // GET: api/drinks
        // Returns all drinks that are active and not soft-deleted.
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<List<Drink>>> GetAll()
        {
            var list = await _drinks.Find(d => d.DeletedUtc == null && d.IsActive).ToListAsync();
            return Ok(list);
        }

        // -------------------------------------------------------------
        // GET: api/drinks/{id}
        // Retrieve a specific drink by its ObjectId.
        // -------------------------------------------------------------
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Drink>> GetById(string id)
        {
            var drink = await _drinks.Find(d => d.Id == id && d.DeletedUtc == null).FirstOrDefaultAsync();
            return drink == null ? NotFound() : Ok(drink);
        }

        // -------------------------------------------------------------
        // POST: api/drinks
        // Create a new drink entry (CreatedUtc auto-set).
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Drink>> Create(Drink model)
        {
            model.CreatedUtc = DateTime.UtcNow;
            await _drinks.InsertOneAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        // -------------------------------------------------------------
        // PUT: api/drinks/{id}
        // Update an existing drink (UpdatedUtc auto-set).
        // -------------------------------------------------------------
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Drink model)
        {
            model.UpdatedUtc = DateTime.UtcNow;
            var result = await _drinks.ReplaceOneAsync(d => d.Id == id, model);
            return result.ModifiedCount == 1 ? NoContent() : NotFound();
        }

        // -------------------------------------------------------------
        // DELETE: api/drinks/{id}
        // Soft delete a drink by setting DeletedUtc timestamp.
        // -------------------------------------------------------------
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var update = Builders<Drink>.Update.Set(d => d.DeletedUtc, DateTime.UtcNow);
            var result = await _drinks.UpdateOneAsync(d => d.Id == id, update);
            return result.ModifiedCount == 1 ? NoContent() : NotFound();
        }
    }
}

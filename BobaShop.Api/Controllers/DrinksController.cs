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
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        // GET: api/v1/Drinks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Drink>>> GetAll([FromQuery] bool includeDeleted = false)
        {
            var filter = includeDeleted
                ? Builders<Drink>.Filter.Empty
                : Builders<Drink>.Filter.Eq(d => d.DeletedUtc, null);

            var list = await _ctx.Drinks
                .Find(filter)
                .SortBy(d => d.Name)
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/v1/Drinks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Drink>> GetById(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest(new { error = "Invalid id format" });

            var drink = await _ctx.Drinks
                .Find(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (drink is null) return NotFound();
            return Ok(drink);
        }

        // POST: api/v1/Drinks   (Admin only)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DrinkCreateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

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

            await _ctx.Drinks.InsertOneAsync(drink);
            return CreatedAtAction(nameof(GetById), new { id = drink.Id }, drink);
        }

        // PUT: api/v1/Drinks/{id}   (Admin only)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DrinkUpdateDto dto)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest(new { error = "Invalid id format" });

            // Force body Id to match the route
            dto.Id = id;

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var filter = Builders<Drink>.Filter.Eq(d => d.Id, id);
            var existed = await _ctx.Drinks.Find(filter).FirstOrDefaultAsync();
            if (existed is null) return NotFound();

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

            await _ctx.Drinks.ReplaceOneAsync(filter, existed);
            return NoContent();
        }


        // DELETE: api/v1/Drinks/{id}   (Admin only, soft delete)
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest(new { error = "Invalid id format" });

            var filter = Builders<Drink>.Filter.Eq(d => d.Id, id);
            var drink = await _ctx.Drinks.Find(filter).FirstOrDefaultAsync();
            if (drink is null) return NotFound();

            if (drink.DeletedUtc is not null)
                return NoContent(); // idempotent

            drink.IsActive = false;
            drink.DeletedUtc = DateTime.UtcNow;
            drink.UpdatedUtc = DateTime.UtcNow;

            await _ctx.Drinks.ReplaceOneAsync(filter, drink);
            return NoContent();
        }
    }
}

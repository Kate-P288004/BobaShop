// ---------------------------------------------------------------
// File: IndexConfigurator.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Purpose:
//   Builds MongoDB indexes for key collections to improve
//   performance of search and filtering queries.
//   - Creates single-field and compound indexes on major collections
//   - Safe to run multiple times (idempotent)
// ---------------------------------------------------------------

using BobaShop.Api.Models;
using MongoDB.Driver;

namespace BobaShop.Api.Data
{
    public static class IndexConfigurator
    {
        public static void EnsureIndexes(MongoDbContext ctx)
        {
            // -------------------------------------------------------
            // 1. Drinks collection
            //    - Index on Name for fast text search
            //    - Compound index on (IsActive, BasePrice)
            // -------------------------------------------------------
            var drinks = ctx.Drinks.Indexes;

            var drinkIndexes = new[]
            {
                // Search by name
                new CreateIndexModel<Drink>(
                    Builders<Drink>.IndexKeys.Ascending(d => d.Name),
                    new CreateIndexOptions { Name = "idx_drink_name" }),

                // Filter by active status and price
                new CreateIndexModel<Drink>(
                    Builders<Drink>.IndexKeys
                        .Ascending(d => d.IsActive)
                        .Ascending(d => d.BasePrice),
                    new CreateIndexOptions { Name = "idx_drink_active_price" })
            };

            drinks.CreateMany(drinkIndexes);

            // -------------------------------------------------------
            // 2. Toppings collection
            //    - Index on Name for quick lookup
            // -------------------------------------------------------
            var toppings = ctx.Toppings.Indexes;
            toppings.CreateOne(
                new CreateIndexModel<Topping>(
                    Builders<Topping>.IndexKeys.Ascending(t => t.Name),
                    new CreateIndexOptions { Name = "idx_topping_name" }));

            // -------------------------------------------------------
            // 3. Orders collection
            //    - Compound index on (CustomerEmail, CreatedUtc)
            // -------------------------------------------------------
            var orders = ctx.Orders.Indexes;
            orders.CreateOne(
                new CreateIndexModel<Order>(
                    Builders<Order>.IndexKeys
                        .Ascending(o => o.CustomerEmail)
                        .Descending(o => o.CreatedUtc),
                    new CreateIndexOptions { Name = "idx_order_customer_created" }));

            // -------------------------------------------------------
            // Optional (if you added Customers collection)
            //    - Index on Email for login lookups
            // -------------------------------------------------------
            try
            {
                var customers = ctx.Customers.Indexes;
                customers.CreateOne(
                    new CreateIndexModel<Customer>(
                        Builders<Customer>.IndexKeys.Ascending(c => c.Email),
                        new CreateIndexOptions { Name = "idx_customer_email", Unique = true }));
            }
            catch
            {
               
            }
        }
    }
}

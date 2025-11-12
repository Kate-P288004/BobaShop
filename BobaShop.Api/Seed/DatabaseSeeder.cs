// -----------------------------------------------------------------------------
// File: Seed/DatabaseSeeder.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Description:
//   Provides idempotent MongoDB seeding logic for initializing core data.
//   Ensures the Drinks and Toppings collections always contain a minimum
//   predefined set of entries without creating duplicates.
//   Safe to execute multiple times on startup (idempotent design).
// -----------------------------------------------------------------------------

using MongoDB.Driver;
using BobaShop.Api.Data;
using BobaShop.Api.Models;
using System;
using System.Collections.Generic;

namespace BobaShop.Api.Seed
{
    // -------------------------------------------------------------------------
    // Class: DatabaseSeeder
    // Purpose:
    //   Contains static helper methods used to seed MongoDB collections
    //   with default data (Drinks and Toppings) during application startup.
    // Notes:
    //   - Uses ReplaceOne with IsUpsert = true to safely upsert by Name.
    //   - This ensures the method can be rerun without causing duplicates.
    // -------------------------------------------------------------------------
    public static class DatabaseSeeder
    {
        // ---------------------------------------------------------------------
        // Entry Point: Seed
        // ---------------------------------------------------------------------
        // Invoked from Program.cs on app startup.
        // Responsible for ensuring all required collections contain data.
        // Add additional seeding methods here as the project expands.
        // ---------------------------------------------------------------------
        public static void Seed(MongoDbContext context)
        {
            EnsureDrinks(context);
            // Future seeders can be added here (e.g., EnsureToppings, EnsureCustomers)
        }

        // ---------------------------------------------------------------------
        // Method: EnsureDrinks
        // ---------------------------------------------------------------------
        // Purpose:
        //   Populates the Drinks collection with 10 essential menu items.
        //   Each item is upserted by its Name to prevent duplication.
        // Logic:
        //   - Builds a “must-have” drink list.
        //   - Checks if each drink already exists.
        //   - Preserves original CreatedUtc for historical accuracy.
        //   - Uses ReplaceOne (with IsUpsert = true) for idempotency.
        // ---------------------------------------------------------------------
        private static void EnsureDrinks(MongoDbContext context)
        {
            var drinks = context.GetCollection<Drink>("Drinks");
            var now = DateTime.UtcNow;

            // Predefined list of drinks (core menu)
            var mustHave = new List<Drink>
            {
                new() { Name = "Classic Milk Tea", Description = "Smooth black tea with milk and pearls.", BasePrice = 6.00m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 50, DefaultIce = 50, IsActive = true, CreatedUtc = now },
                new() { Name = "Brown Sugar Boba", Description = "Rich milk tea with brown sugar syrup and pearls.", BasePrice = 7.00m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 75, DefaultIce = 50, IsActive = true, CreatedUtc = now },
                new() { Name = "Taro Milk Tea", Description = "Sweet taro blended with creamy milk.", BasePrice = 6.50m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 75, DefaultIce = 50, IsActive = true, CreatedUtc = now },
                new() { Name = "Matcha Milk Tea", Description = "Japanese matcha with milk and light sweetness.", BasePrice = 7.00m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 60, DefaultIce = 40, IsActive = true, CreatedUtc = now },
                new() { Name = "Thai Milk Tea", Description = "Thai black tea with condensed milk and spice.", BasePrice = 6.80m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 80, DefaultIce = 60, IsActive = true, CreatedUtc = now },
                new() { Name = "Mango Green Tea", Description = "Green tea with mango flavour and pulp.", BasePrice = 6.20m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 75, DefaultIce = 75, IsActive = true, CreatedUtc = now },
                new() { Name = "Passionfruit Green Tea", Description = "Tangy passionfruit with jasmine green tea.", BasePrice = 6.20m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 70, DefaultIce = 70, IsActive = true, CreatedUtc = now },
                new() { Name = "Dirty Brown Sugar Cream Cap", Description = "Brown sugar boba with creamy foam topping.", BasePrice = 7.50m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 80, DefaultIce = 60, IsActive = true, CreatedUtc = now },
                new() { Name = "Matcha Strawberry Latte", Description = "Layered strawberry puree and matcha milk.", BasePrice = 7.80m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 65, DefaultIce = 60, IsActive = true, CreatedUtc = now },
                new() { Name = "Oreo Cocoa Crush", Description = "Cocoa milkshake with crushed Oreo cookies.", BasePrice = 7.50m, MediumUpcharge = 0.50m, LargeUpcharge = 1.00m, DefaultSugar = 75, DefaultIce = 80, IsActive = true, CreatedUtc = now }
            };

            foreach (var d in mustHave)
            {
                // Find existing record by unique Name
                var filter = Builders<Drink>.Filter.Eq(x => x.Name, d.Name);
                var existing = drinks.Find(filter).FirstOrDefault();

                // Preserve Id and CreatedUtc for audit consistency
                if (existing != null)
                {
                    d.Id = existing.Id;
                    if (existing.CreatedUtc != default)
                        d.CreatedUtc = existing.CreatedUtc;
                }

                // Upsert ensures either an update or insert (never duplicates)
                drinks.ReplaceOne(filter, d, new ReplaceOptions { IsUpsert = true });
            }
        }

        // ---------------------------------------------------------------------
        // Method: EnsureToppings
        // ---------------------------------------------------------------------
        // Purpose:
        //   Seeds the Toppings collection with default topping options.
        //   Similar to EnsureDrinks, it uses upsert to prevent duplicates.
        // Notes:
        //   - Not currently called by Seed(), but can be enabled anytime.
        // ---------------------------------------------------------------------
        private static void EnsureToppings(MongoDbContext context)
        {
            var toppings = context.GetCollection<Topping>("Toppings");

            // Base topping options with realistic pricing
            var mustHave = new List<Topping>
            {
                new() { Name = "Pearls", Price = 0.80m, IsActive = true },
                new() { Name = "Coffee Jelly", Price = 0.80m, IsActive = true },
                new() { Name = "Lychee Jelly", Price = 0.80m, IsActive = true },
                new() { Name = "Popping Mango", Price = 1.00m, IsActive = true },
                new() { Name = "Cheese Foam", Price = 1.20m, IsActive = true }
            };

            foreach (var t in mustHave)
            {
                var filter = Builders<Topping>.Filter.Eq(x => x.Name, t.Name);
                toppings.ReplaceOne(filter, t, new ReplaceOptions { IsUpsert = true });
            }
        }
    }
}

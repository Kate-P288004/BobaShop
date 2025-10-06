// -----------------------------------------------------------------------------
// File: DatabaseSeeder.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: October 2025
// Assessment: Diploma of IT – Application Development Project
// Description:
// Seeds initial MongoDB collections (Drinks) when the application first runs.
// -----------------------------------------------------------------------------

using MongoDB.Driver;
using BobaShop.Api.Data;
using BobaShop.Api.Models;

namespace BobaShop.Api.Seed
{
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Inserts initial sample data if collections are empty.
        /// </summary>
        public static void Seed(MongoDbContext context)
        {
            var drinks = context.GetCollection<Drink>("Drinks");

            // Only seed if collection is empty
            if (!drinks.Find(_ => true).Any())
            {
                var seedData = new List<Drink>
                {
                    new Drink
                    {
                        Name = "Classic Milk Tea",
                        Description = "Black tea with milk and tapioca pearls.",
                        BasePrice = 6.00m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 50,
                        DefaultIce = 50,
                        IsActive = true,
                        CreatedUtc = DateTime.UtcNow
                    },
                    new Drink
                    {
                        Name = "Taro Milk Tea",
                        Description = "Sweet taro flavour blended with creamy milk.",
                        BasePrice = 6.50m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 75,
                        DefaultIce = 50,
                        IsActive = true,
                        CreatedUtc = DateTime.UtcNow
                    },
                    new Drink
                    {
                        Name = "Matcha Latte",
                        Description = "Japanese green tea with milk and a hint of vanilla.",
                        BasePrice = 7.00m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 60,
                        DefaultIce = 40,
                        IsActive = true,
                        CreatedUtc = DateTime.UtcNow
                    }
                };

                drinks.InsertMany(seedData);
            }
        }
    }
}

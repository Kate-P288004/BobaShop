// -----------------------------------------------------------------------------
// File: DatabaseSeeder.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Seeds the MongoDB database with initial sample data when the application
//   starts. This ensures the Drinks collection contains default menu items
//   for testing and demonstration purposes. Implements seeding logic for the
//   NoSQL environment using the MongoDB .NET Driver.
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
    //   Provides static seeding logic for the application database.
    //   If the Drinks collection is empty, a predefined list of sample
    //   drinks is inserted into MongoDB on startup.
    // Mapping: ICTPRG554 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Inserts default drink data into MongoDB if the collection is empty.
        /// This method is typically called from Program.cs during application startup.
        /// </summary>
        /// <param name="context">The MongoDbContext instance used to access collections.</param>
        public static void Seed(MongoDbContext context)
        {
            // Access the Drinks collection directly
            var drinks = context.GetCollection<Drink>("Drinks");

            // Only seed if the collection has no existing records
            if (!drinks.Find(_ => true).Any())
            {
                var now = DateTime.UtcNow;

                // -------------------------------------------------------------
                // Seed Data: Sample drinks used for development/testing
                // -------------------------------------------------------------
                var seedData = new List<Drink>
                {
                    // ------------------ CORE MILK TEAS ------------------
                    new Drink
                    {
                        Name = "Classic Milk Tea",
                        Description = "Smooth black tea with milk and chewy tapioca pearls.",
                        BasePrice = 6.00m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 50,
                        DefaultIce = 50,
                        IsActive = true,
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Brown Sugar Boba",
                        Description = "Rich milk tea swirled with brown sugar syrup and pearls.",
                        BasePrice = 7.00m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 75,
                        DefaultIce = 50,
                        IsActive = true,
                        CreatedUtc = now
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
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Matcha Milk Tea",
                        Description = "Japanese matcha with smooth milk and light sweetness.",
                        BasePrice = 7.00m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 60,
                        DefaultIce = 40,
                        IsActive = true,
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Thai Milk Tea",
                        Description = "Strong brewed Thai black tea with condensed milk and spice.",
                        BasePrice = 6.80m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 80,
                        DefaultIce = 60,
                        IsActive = true,
                        CreatedUtc = now
                    },

                    // ------------------ FRUIT / SIGNATURE TEAS ------------------
                    new Drink
                    {
                        Name = "Mango Green Tea",
                        Description = "Refreshing green tea with mango flavour and pulp.",
                        BasePrice = 6.20m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 75,
                        DefaultIce = 75,
                        IsActive = true,
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Passionfruit Green Tea",
                        Description = "Tangy passionfruit blended with jasmine green tea.",
                        BasePrice = 6.20m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 70,
                        DefaultIce = 70,
                        IsActive = true,
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Dirty Brown Sugar Cream Cap",
                        Description = "Brown sugar boba with a rich creamy foam topping.",
                        BasePrice = 7.50m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 80,
                        DefaultIce = 60,
                        IsActive = true,
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Matcha Strawberry Latte",
                        Description = "Layered strawberry puree and matcha milk blend.",
                        BasePrice = 7.80m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 65,
                        DefaultIce = 60,
                        IsActive = true,
                        CreatedUtc = now
                    },
                    new Drink
                    {
                        Name = "Oreo Cocoa Crush",
                        Description = "Creamy cocoa milkshake with crushed Oreo cookies.",
                        BasePrice = 7.50m,
                        MediumUpcharge = 0.50m,
                        LargeUpcharge = 1.00m,
                        DefaultSugar = 75,
                        DefaultIce = 80,
                        IsActive = true,
                        CreatedUtc = now
                    }
                };

                // Insert all records in one batch operation
                drinks.InsertMany(seedData);
            }
        }
    }
}

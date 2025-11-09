// ---------------------------------------------------------------
// File: MongoDbContext.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: October 2025
// Purpose:
//   Provides MongoDB connectivity for the API layer.
//   - Reads settings from appsettings.json ("Mongo" section)
//   - Exposes typed collections for CRUD operations
//   - Supports dependency injection through IOptions<MongoSettings>
// ---------------------------------------------------------------

using BobaShop.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BobaShop.Api.Data
{
    // -----------------------------------------------------------
    // MongoDB configuration settings (bound from appsettings.json)
    // -----------------------------------------------------------
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "BobaShopDb";
    }

    // -----------------------------------------------------------
    // MongoDbContext: central data access point for the API
    // -----------------------------------------------------------
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        // Optional constants (for clarity and to avoid typos)
        private const string CustomersCollection = "customers";
        private const string DrinksCollection = "Drinks";
        private const string ToppingsCollection = "toppings";
        private const string OrdersCollection = "orders";

        // -------------------------------------------------------
        // Constructor: uses dependency injection for settings
        // -------------------------------------------------------
        public MongoDbContext(IOptions<MongoSettings> options)
        {
            var cfg = options.Value;

            // Create MongoDB client
            var client = new MongoClient(cfg.ConnectionString);

            // Connect to specified database
            _database = client.GetDatabase(cfg.DatabaseName);
        }

        // -------------------------------------------------------
        // Explicit collections used by controllers
        // -------------------------------------------------------
        public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>(CustomersCollection);
        public IMongoCollection<Drink> Drinks => _database.GetCollection<Drink>(DrinksCollection);
        public IMongoCollection<Topping> Toppings => _database.GetCollection<Topping>(ToppingsCollection);
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>(OrdersCollection);

        // -------------------------------------------------------
        // Generic helper: allows retrieving any collection by name
        // -------------------------------------------------------
        public IMongoCollection<T> GetCollection<T>(string name)
            => _database.GetCollection<T>(name);
    }
}

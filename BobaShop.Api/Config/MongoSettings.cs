// -----------------------------------------------------------------------------
// File: MongoSettings.cs
// Project: BobaShop.Api (BoBatastic)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Purpose:
//   Provides strongly-typed configuration settings for MongoDB connectivity.
//   Used by dependency injection via IOptions<MongoSettings> in Program.cs.
//   Includes connection string, database name, SSL toggle, and default
//   collection names for Customers, Drinks, Toppings, and Orders.
// -----------------------------------------------------------------------------

namespace BobaShop.Api.Config
{
    // -------------------------------------------------------------------------
    // Class: MongoSettings
    // Purpose:
    //   Stores MongoDB connection parameters and collection name mappings.
    //   Bound automatically from appsettings.json "Mongo" section.
    // -------------------------------------------------------------------------
    public sealed class MongoSettings
    {
        // Connection string for MongoDB (e.g., "mongodb://localhost:27017")
        // Used by MongoClient in MongoDbContext to establish the connection.
        public string ConnectionString { get; set; } = "";

        // Logical name of the database to use within MongoDB server.
        // All collections (e.g., drinks, toppings) belong to this database.
        public string DatabaseName { get; set; } = "";

        // Indicates whether SSL/TLS encryption is enabled for the connection.
        // Default is false for local development, true in production.
        public bool EnableSSL { get; set; } = false;

        // ---------------------------------------------------------------------
        // Nested Class: Collections
        // Purpose:
        //   Defines default names for all MongoDB collections used by the app.
        //   This ensures consistent references across controllers and services.
        // ---------------------------------------------------------------------
        public class Collections
        {
            // MongoDB collection name for customer documents
            public string Customers { get; set; } = "customers";

            // MongoDB collection name for drink documents
            public string Drinks { get; set; } = "Drinks";

            // MongoDB collection name for topping documents
            public string Toppings { get; set; } = "toppings";

            // MongoDB collection name for order documents
            public string Orders { get; set; } = "orders";
        }

        // Property that holds all collection name configurations.
        // Instantiated by default to avoid null references during injection.
        public Collections CollectionNames { get; set; } = new();
    }
}

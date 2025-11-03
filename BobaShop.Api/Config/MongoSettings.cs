namespace BobaShop.Api.Config
{
    public sealed class MongoSettings
    {
        public string ConnectionString { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public bool EnableSSL { get; set; } = false;

        public class Collections
        {
            public string Customers { get; set; } = "customers";
            public string Drinks { get; set; } = "drinks";
            public string Toppings { get; set; } = "toppings";
            public string Orders { get; set; } = "orders";
        }

        public Collections CollectionNames { get; set; } = new();
    }
}

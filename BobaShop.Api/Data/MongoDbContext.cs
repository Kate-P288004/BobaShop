using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BobaShop.Api.Data
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "BobaShopDb";
    }

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IOptions<MongoSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.DatabaseName);
        }
        public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);
    }
}

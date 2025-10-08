using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BobaShop.Api.Models
{
    public class Topping
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = default!; 

        [BsonElement("price")]
        public decimal Price { get; set; } = 0.80m;  // surcharge

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // Magic Three Dates
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("deletedAt")]
        public DateTime? DeletedAt { get; set; }
    }
}

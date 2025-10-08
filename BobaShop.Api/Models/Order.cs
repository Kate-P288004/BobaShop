using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BobaShop.Api.Models;

/// <summary>
/// Represents a customer order in the BobaShop API.
/// Includes drinks, total amount, and timestamps.
/// </summary>
public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// The email of the customer placing the order.
    /// </summary>
    [BsonElement("customerEmail")]
    public string CustomerEmail { get; set; } = default!;

    /// <summary>
    /// The list of ordered drink IDs.
    /// </summary>
    [BsonElement("drinkIds")]
    public List<string> DrinkIds { get; set; } = new();

    /// <summary>
    /// The list of selected topping IDs (optional).
    /// </summary>
    [BsonElement("toppingIds")]
    public List<string>? ToppingIds { get; set; }

    /// <summary>
    /// The order total (in AUD).
    /// </summary>
    [BsonElement("total")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Total { get; set; }

    /// <summary>
    /// Order status (New, InProgress, Completed, Cancelled).
    /// </summary>
    [BsonElement("status")]
    public string Status { get; set; } = "New";

    /// <summary>
    /// Created timestamp (Magic Three Dates requirement).
    /// </summary>
    [BsonElement("createdUtc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp (Magic Three Dates requirement).
    /// </summary>
    [BsonElement("updatedUtc")]
    public DateTime? UpdatedUtc { get; set; }

    /// <summary>
    /// Soft delete timestamp (Magic Three Dates requirement).
    /// </summary>
    [BsonElement("deletedUtc")]
    public DateTime? DeletedUtc { get; set; }
}

namespace NetServer.Models;
using System.Text.Json.Serialization;

public class ItemAddRequest
{
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("itemId")]
    public required string ItemId { get; set; }
}
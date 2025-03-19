namespace NetServer.Models;
using System.Text.Json.Serialization;

public class ItemAddRequest
{
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    // last fm specific - if the MBID is missing.
    public string? Title { get; set; }   
    public string? Artist { get; set; }
    
}
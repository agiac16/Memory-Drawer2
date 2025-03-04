using System.Text.Json.Serialization;

public class ItemRatingRequest
{
    [JsonPropertyName("rating")]
    public required float Rating { get; set; }
}
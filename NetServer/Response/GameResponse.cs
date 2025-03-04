namespace NetServer.Models;
using System.Text.Json.Serialization;

// nested as Results -> with name and image -> image is nested with medium url inside

public class GiantBombResponse
{
    [JsonPropertyName("results")]
    public GameResponse? Results { get; set; }
}

public class GameResponse
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("image")]
    public required ImageResponse Image { get; set; }
}

public class ImageResponse { 
    [JsonPropertyName("medium_url")]
    public required string MediumUrl { get; set; }
}
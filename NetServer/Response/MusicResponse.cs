namespace NetServer.Models;
using System.Text.Json.Serialization;
public class MusicResponse
{
    [JsonPropertyName("album")]
    public AlbumDetails? Album { get; set; }
}

// all this info is nested within AlbumDetails

public class AlbumDetails
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("artist")]
    public required string Artist { get; set; }

    [JsonPropertyName("image")]
    public List<ImageInfo>? Images { get; set; }
}

public class ImageInfo
{
    [JsonPropertyName("#text")]
    public string? Url { get; set; }

    public string? Size { get; set; }
}

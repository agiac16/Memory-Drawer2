namespace NetServer.Models;
using System.Text.Json.Serialization;

public class GoogleBooksResponse
{
    [JsonPropertyName("items")]
    public List<GoogleBookItem>? Items { get; set; }
}

public class GoogleBookItem
{
    [JsonPropertyName("volumeInfo")]
    public GoogleBookVolumeInfo? VolumeInfo { get; set; }
}

public class GoogleBookVolumeInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("authors")]
    public List<string>? Authors { get; set; }

    [JsonPropertyName("imageLinks")]
    public GoogleBookImageLinks? ImageLinks { get; set; }
}

public class GoogleBookImageLinks
{
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}

// used for single item search || how it returns 
public class GoogleBook
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volumeInfo")]
    public GoogleBookVolumeInfo? VolumeInfo { get; set; }
}
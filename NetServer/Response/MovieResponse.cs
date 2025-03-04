namespace NetServer.Models;
using System.Text.Json.Serialization;
public class MovieResponse
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("poster_path")]
    public required string PosterPath { get; set; }
}
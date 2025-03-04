using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
using ZstdSharp.Unsafe;

[ApiController]
[Route("api/music")]
public class MusicController : Controller
{
    private readonly IMusicRepository _musicRepository;
    private readonly HttpClient _httpClient;
    private readonly string _musicApiKey;

    public MusicController(IMusicRepository musicRepository, IConfiguration configuration)
    {
        _musicRepository = musicRepository;
        _httpClient = new HttpClient();
        _musicApiKey = configuration["LastFM:ApiKey"]
                        ?? throw new ArgumentNullException("Invalid API key (LastFm)");
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddMusicToUser([FromBody] ItemAddRequest request) {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrEmpty(request.ItemId)) {
            return BadRequest("User ID and Item ID are required");
        }

        string url = $"http://ws.audioscrobbler.com/2.0/?method=album.getInfo&api_key={_musicApiKey}&mbid={request.ItemId}&format=json";        
        var res = await _httpClient.GetStringAsync(url); // return body as string 

        if (string.IsNullOrEmpty(res))
        {
            return NotFound("Item not found in API.");
        }

        var musicDetails = JsonSerializer.Deserialize<MusicResponse>(res); // maps it to this response class
        
        if (musicDetails?.Album == null) { 
            return BadRequest("Invalid Item");
        }

        string artworkUrl = musicDetails.Album.Images?.LastOrDefault()?.Url ?? "";

        var music = new Music {
            UserId = request.UserId,
            ApiId = request.ItemId,
            Title = musicDetails.Album.Name ?? "Unknown Title",
            Artist = musicDetails.Album.Artist ?? "Unknown Artist",
            Artwork = artworkUrl,
            Rating = null
        };

        await _musicRepository.AddMusicAsync(request.UserId, music);
        return Ok($"Music '{music.Title}' added successfully.");
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserMusic([FromRoute] string userId) {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("User ID is required");

        var userMusic = await _musicRepository.GetAllMusicByUserAsync(userId);

        if (userMusic == null || !userMusic.Any()) {
            return NotFound(new { message = "no music found for user"});
        }

        return Ok(userMusic); // json
    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateMusicRating(string userId, string itemId, [FromBody] ItemRatingRequest request) { 
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemId))
        return BadRequest("User ID and Item ID cannot be empty.");

        if (request.Rating < 0 || request.Rating > 5)
            return BadRequest("Invalid rating value. Rating must be between 0 and 5.");

        bool success = await _musicRepository.UpdateMusicRatingAsync(userId, itemId, request.Rating);

        if (!success) return NotFound("Item not found");

        return Ok("Rating updated success");
    }

    [HttpDelete("{userId}/{musicId}")]
    public async Task<ActionResult> DeleteMusic(string userId, string musicId) {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(musicId)) 
            return BadRequest("User ID and Movie ID are required");
        
        var success = await _musicRepository.DeleteMusicAsync(userId, musicId);
        if (!success) return NotFound("Item not found");

        return Ok("Item deleted");
    }

     // search function or in service not sure yet
}

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;

[EnableCors("_myAllowSpecificOrigins")]
[ApiController]
[Route("api/music")]
public class MusicController : Controller
{
    private readonly SearchService _searchService;
    private readonly IMediaRepository<Music> _musicRepository;
    private readonly HttpClient _httpClient;
    private readonly string _musicApiKey;

    public MusicController(IMediaRepository<Music> musicRepository, IConfiguration configuration, SearchService searchService)
    {
        _musicRepository = musicRepository;
        _searchService = searchService;
        _httpClient = new HttpClient();
        _musicApiKey = configuration["LastFM:ApiKey"]
                        ?? throw new ArgumentNullException("Invalid API key (LastFm)");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMusic([FromQuery] string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return BadRequest("Invalid Query");

        try
        {
            var res = await _searchService.SearchMusicAsync(title);
            if (res == null) return NotFound("No music matching search");

            return Ok(res);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error searching music: {e.Message}");
            return StatusCode(500, "An error occurred while searching for music.");
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddMusicToUser([FromBody] ItemAddRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest("User ID is required");

        try
        {
            string url;

            if (!string.IsNullOrWhiteSpace(request.ItemId) && request.ItemId.ToLower() != "null") // ✅ Prevents `mbid=null`
            {
                url = $"http://ws.audioscrobbler.com/2.0/?method=album.getInfo&api_key={_musicApiKey}&mbid={request.ItemId}&format=json";
            }
            else if (!string.IsNullOrWhiteSpace(request.Title) && !string.IsNullOrWhiteSpace(request.Artist))
            {
                url = $"http://ws.audioscrobbler.com/2.0/?method=album.getInfo&api_key={_musicApiKey}&artist={Uri.EscapeDataString(request.Artist)}&album={Uri.EscapeDataString(request.Title)}&format=json";
            }
            else
            {
                return BadRequest("Either Item ID (MBID) or Album Title & Artist are required.");
            }

            Console.WriteLine($"Fetching data from: {url}");

            var res = await _httpClient.GetStringAsync(url);

            if (string.IsNullOrEmpty(res))
                return NotFound("Item not found in API.");

            var musicDetails = JsonSerializer.Deserialize<MusicResponse>(res, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (musicDetails?.Album == null)
                return BadRequest("Invalid Item");

            string artworkUrl = musicDetails.Album.Images?.LastOrDefault()?.Url ?? "";

            var music = new Music
            {
                UserId = request.UserId,
                ApiId = request.ItemId ?? $"{request.Artist}-{request.Title}".Replace(" ", "-").ToLower(), // ✅ Generate ID if MBID is missing
                Title = musicDetails.Album.Name ?? request.Title ?? "Unknown Title",
                Artist = musicDetails.Album.Artist ?? request.Artist ?? "Unknown Artist",
                Artwork = artworkUrl,
                Rating = null,
                AddedAt = DateTime.UtcNow
            };

            await _musicRepository.AddItemToUserAsync(request.UserId, music);
            return Ok(new { message = $"{music.Title} added successfully." });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding music: {e.Message}");
            return StatusCode(500, "An error occurred while adding the music.");
        }
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserMusic([FromRoute] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("User ID is required");

        try
        {
            var userMusic = await _musicRepository.GetAllByUserAsync(userId);
            if (userMusic == null || !userMusic.Any())
                return NotFound(new { message = "No music found for user" });

            return Ok(userMusic);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching user music: {e.Message}");
            return StatusCode(500, "An error occurred while retrieving user music.");
        }
    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateMusicRating(string userId, string itemId, [FromBody] ItemRatingRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemId))
            return BadRequest("User ID and Item ID cannot be empty.");

        if (request.Rating < 0 || request.Rating > 5)
            return BadRequest("Invalid rating value. Rating must be between 0 and 5.");

        try
        {
            bool success = await _musicRepository.UpdateRatingAsync(userId, itemId, request.Rating);
            if (!success) return NotFound("Item not found");

            return Ok("Rating updated successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating music rating: {e.Message}");
            return StatusCode(500, "An error occurred while updating the music rating.");
        }
    }

    [HttpDelete("{userId}/{musicId}")]
    public async Task<ActionResult> DeleteMusic(string userId, string musicId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(musicId))
            return BadRequest("User ID and Music ID are required");

        try
        {
            var success = await _musicRepository.DeleteItemAsync(userId, musicId);
            if (!success) return NotFound("Item not found");

            return Ok("Item deleted successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting music: {e.Message}");
            return StatusCode(500, "An error occurred while deleting the music.");
        }
    }
}
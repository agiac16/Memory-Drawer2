using System.Runtime.CompilerServices;
using System.Text.Json;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
using ZstdSharp.Unsafe;
using Microsoft.AspNetCore.Cors;

[EnableCors("_myAllowSpecificOrigins")]
[ApiController]
[Route("api/games")]
public class GameController : Controller
{
    private readonly SearchService _searchService;
    private readonly IMediaRepository<Game> _gameRepository;
    private readonly HttpClient _httpClient;
    private readonly string _gameApiKey;

    public GameController(IMediaRepository<Game> gameRepository, IConfiguration configuration, SearchService searchService)
    {
        _gameRepository = gameRepository;
        _searchService = searchService; 
        _httpClient = new HttpClient();
        _gameApiKey = configuration["GiantBomb:ApiKey"]
                        ?? throw new ArgumentNullException("Invopk alid API key (TMDB)");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchGames([FromQuery] string title) { 
        if (string.IsNullOrWhiteSpace(title)) return BadRequest("Invalid Query");

        var res = await _searchService.SearchGamesAsync(title); 
        if (res == null) return NotFound("No games matching search");

        return Ok(res);
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddGameToUser([FromBody] ItemAddRequest request)
    {
        if (request == null) return BadRequest("Invalid JSON");

        Console.WriteLine($"Received UserId: {request.UserId}, ItemId: {request.ItemId}");


        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ItemId))
        {
            return BadRequest("User Id and Game Id are required");
        }
        
        // Ensure JSON response format
        string url = $"https://www.giantbomb.com/api/game/{request.ItemId}/?api_key={_gameApiKey}&format=json";

        // Create HTTP request with headers
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Add("User-Agent", "MemoryDrawerApp/1.0"); // headers needed
        httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, $"Error fetching game from GiantBomb API: {response.ReasonPhrase}");
        }

        // read and parse
        var res = await response.Content.ReadAsStringAsync();
        Console.WriteLine("API" + res);
        var gameData = JsonSerializer.Deserialize<GiantBombResponse>(res, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (gameData?.Results == null) return BadRequest("Invalid game data received");

        GameResponse gameDetails = gameData.Results;
        string artworkUrl = gameDetails.Image?.MediumUrl ?? "";

        var game = new Game
        {
            UserId = request.UserId,
            ApiId = request.ItemId,
            Title = gameDetails.Name,
            Artwork = artworkUrl,
            Rating = null
        };

        await _gameRepository.AddItemToUserAsync(request.UserId, game);
        return Ok(new { message = $"{game.Title} added successfully." });
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserGames([FromRoute] string userId) {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("User ID is required");

        var userGames = await _gameRepository.GetAllByUserAsync(userId);

        if (userGames == null || !userGames.Any()) 
            return NotFound(new { message = "no games found for user"});
        
        return Ok(userGames);

    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateGameRating(string userId, string itemId, [FromBody] ItemRatingRequest request) { 
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(itemId))
            return BadRequest("User Id and Item Id cannot be empty");
        
        if (request.Rating < 0 || request.Rating > 5) 
            return BadRequest("Invalid rating. Rating must be between 0 and 5.");
        
        bool success = await _gameRepository.UpdateRatingAsync(userId, itemId, request.Rating); 

        if (!success) return NotFound("item not found"); 

        return Ok("Rating update successfully");
    }

    [HttpDelete("{userId}/{gameId}")]
    public async Task<ActionResult> DeleteGame(string userId, string gameId) {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(gameId)) 
            return BadRequest("User Id and Game Id are required.");
        
        var success = await _gameRepository.DeleteItemAsync(userId, gameId); 
        if (!success) return NotFound("Game not found");

        return Ok("Game deleted successfully.");
    }
}

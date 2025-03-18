using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
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
                        ?? throw new ArgumentNullException("Invalid API key (GiantBomb)");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchGames([FromQuery] string title)
    { 
        if (string.IsNullOrWhiteSpace(title)) return BadRequest("Invalid Query");

        try
        {
            var res = await _searchService.SearchGamesAsync(title);
            if (res == null) return NotFound("No games matching search");

            return Ok(res);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error searching games: {e.Message}");
            return StatusCode(500, "An error occurred while searching for games.");
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddGameToUser([FromBody] ItemAddRequest request)
    {
        if (request == null) return BadRequest("Invalid JSON");

        Console.WriteLine($"Received UserId: {request.UserId}, ItemId: {request.ItemId}");

        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ItemId))
            return BadRequest("User Id and Game Id are required");

        try
        {
            // Ensure JSON response format
            string url = $"https://www.giantbomb.com/api/game/{request.ItemId}/?api_key={_gameApiKey}&format=json";

            // Create HTTP request with headers
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("User-Agent", "MemoryDrawerApp/1.0"); // headers needed
            httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, $"Error fetching game from GiantBomb API: {response.ReasonPhrase}");

            // Read and parse API response
            var res = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response: " + res);
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
                Rating = null,
                AddedAt = DateTime.UtcNow
            };

            await _gameRepository.AddItemToUserAsync(request.UserId, game);
            return Ok(new { message = $"{game.Title} added successfully." });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding game: {e.Message}");
            return StatusCode(500, "An error occurred while adding the game.");
        }
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserGames([FromRoute] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("User ID is required");

        try
        {
            var userGames = await _gameRepository.GetAllByUserAsync(userId);
            if (userGames == null || !userGames.Any()) 
                return NotFound(new { message = "No games found for user" });

            return Ok(userGames);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching user games: {e.Message}");
            return StatusCode(500, "An error occurred while retrieving user games.");
        }
    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateGameRating(string userId, string itemId, [FromBody] ItemRatingRequest request)
    { 
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(itemId))
            return BadRequest("User Id and Item Id cannot be empty");

        if (request.Rating < 0 || request.Rating > 5) 
            return BadRequest("Invalid rating. Rating must be between 0 and 5.");

        try
        {
            bool success = await _gameRepository.UpdateRatingAsync(userId, itemId, request.Rating); 
            if (!success) return NotFound("Item not found"); 

            return Ok("Rating updated successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating game rating: {e.Message}");
            return StatusCode(500, "An error occurred while updating the game rating.");
        }
    }

    [HttpDelete("{userId}/{gameId}")]
    public async Task<ActionResult> DeleteGame(string userId, string gameId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(gameId)) 
            return BadRequest("User Id and Game Id are required.");

        try
        {
            var success = await _gameRepository.DeleteItemAsync(userId, gameId); 
            if (!success) return NotFound("Game not found");

            return Ok("Game deleted successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting game: {e.Message}");
            return StatusCode(500, "An error occurred while deleting the game.");
        }
    }
}
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
using ZstdSharp.Unsafe;

[ApiController]
[Route("api/movies")]
public class MovieController : Controller
{
    private readonly SearchService _searchService;
    private readonly IMovieRepository _movieRepository;
    private readonly HttpClient _httpClient;
    private readonly string _tmdbApiKey;

    public MovieController(IMovieRepository movieRepository, IConfiguration configuration, SearchService searchService)
    {
        _movieRepository = movieRepository;
        _searchService = searchService; 
        _httpClient = new HttpClient();
        _tmdbApiKey = configuration["TMDB:ApiKey"]
                        ?? throw new ArgumentNullException("Invalid API key (TMDB)");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMovies([FromQuery] string title) { 
        if (string.IsNullOrWhiteSpace(title)) return BadRequest("Invalid Query");

        var res = await _searchService.SearchMoviesAsync(title); 
        if (res == null) return NotFound("No music matching search");

        return Ok(res);
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddMovieToUser([FromBody] ItemAddRequest request)
    {
        if (request is null)
        {
            return BadRequest("Invalid JSON format.");
        }

        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ItemId))
        {
            return BadRequest("User ID and Movie ID are required.");
        }

        // Call TMDB API
        string url = $"https://api.themoviedb.org/3/movie/{request.ItemId}?api_key={_tmdbApiKey}";
        var res = await _httpClient.GetStringAsync(url);

        if (string.IsNullOrEmpty(res))
        {
            return NotFound("Movie not found in TMDB.");
        }

        var movieDetails = JsonSerializer.Deserialize<MovieResponse>(res);

        if (movieDetails == null)
        {
            return BadRequest("Invalid movie data received.");
        }

        string baseImageUrl = "https://image.tmdb.org/t/p/w500";
        string posterUrl = string.IsNullOrEmpty(movieDetails.PosterPath) ? "" : $"{baseImageUrl}{movieDetails.PosterPath}";

        var movie = new Movie
        {
            UserId = request.UserId,
            ApiId = request.ItemId,
            Title = movieDetails.Title ?? "Unknown Title",
            PosterPath = posterUrl,
            Rating = null
        };

        await _movieRepository.AddMovieToUserAsync(request.UserId, movie);
        return Ok($"'{movie.Title}' added successfully.");
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserMovies([FromRoute] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User Id is required");
        }
        var userMovies = await _movieRepository.GetAllByUserAsync(userId);

        if (userMovies == null || !userMovies.Any())
        {
            return NotFound(new { message = "No movies found for this user." });
        }

        return Ok(userMovies); // json list
    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateMovieRating(string userId, string itemId, [FromBody] ItemRatingRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemId))
            return BadRequest("User ID and Item ID cannot be empty.");

        if (request.Rating < 0 || request.Rating > 5)
            return BadRequest("Invalid rating value. Rating must be between 0 and 5.");

        bool success = await _movieRepository.UpdateMovieAsync(userId, itemId, request.Rating);

        if (!success) return NotFound("Item not found");

        return Ok("Rating updated success");
    }

    [HttpDelete("{userId}/{movieId}")]
    public async Task<ActionResult> DeleteMovie(string userId, string movieId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(movieId))
        {
            return BadRequest("User ID and Movie ID are required.");
        }

        var success = await _movieRepository.DeleteMovieAsync(userId, movieId);
        if (!success) return NotFound("Movie not found");

        return Ok("Movie deleted");
    }


    // search... here or service?
}
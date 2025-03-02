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
    private readonly IMovieRepository _movieRepository;
    private readonly HttpClient _httpClient;
    private readonly string _tmdbApiKey;

    public MovieController(IMovieRepository movieRepository, IConfiguration configuration)
    {
        _movieRepository = movieRepository;
        _httpClient = new HttpClient();
        _tmdbApiKey = configuration["TMDB:ApiKey"]
                        ?? throw new ArgumentNullException("Invalid API key (TMDB)");
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddMovieToUser([FromBody] MovieAddRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.MovieId))
        {
            return BadRequest("User ID and Movie ID are required.");
        }

        // call API
        string url = $"https://api.themoviedb.org/3/movie/{request.MovieId}?api_key={_tmdbApiKey}";
        var res = await _httpClient.GetStringAsync(url);

        if (string.IsNullOrEmpty(url))
        {
            return NotFound("Movie not found in TMDB.");
        }

        var movieDetails = JsonSerializer.Deserialize<Dictionary<string, object>>(res);

        if (movieDetails == null)
        {
            return NotFound("Invalid movie data received.");
        }

        // json return mapped to Movie model 
        var movie = new Movie
        {
            UserId = request.UserId,
            ApiId = request.MovieId,
            Title = movieDetails["title"]?.ToString() ?? "Unknown Title",
            PosterPath = movieDetails["poster_path"]?.ToString() ?? "",
            Rating = null
        };

        await _movieRepository.AddMovieToUserAsync(request.UserId, movie);
        return Ok($" `{movie.Title}` added successfully.");
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

    [HttpPut("{userId}/rate")]
    public async Task<IActionResult> UpdateMovieRating(string userId, [FromBody] Dictionary<string, object> request)
    {
        if (!request.TryGetValue("movieId", out var movieIdObj) || !request.TryGetValue("rating", out var ratingObj))
        {
            return BadRequest("Invalid request. Ensure movieId and rating are included.");
        }

        string movieId = movieIdObj.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(movieId)) return BadRequest("Invalid request. movieId cannot be empty.");


        if (!float.TryParse(ratingObj.ToString(), out float rating))
        {
            return BadRequest("Invalid rating value.");
        }

        var success = await _movieRepository.UpdateMovieAsync(userId, movieId, rating);

        if (!success)
        {
            return NotFound("Movie not found.");
        }

        return Ok("Movie rating updated successfully.");
    }

    [HttpDelete("{userId}/{movieId}")]
    public async Task<ActionResult> DeleteMovie(string userId, string movieId) { 
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(movieId)) { 
            return BadRequest("User ID and Movie ID are required.");
        }

        var success = await _movieRepository.DeleteMovieAsync(userId, movieId);
        if (!success) return NotFound("Movie not found");

        return Ok("Movie deleted");
    }


    // search... here or service?
}
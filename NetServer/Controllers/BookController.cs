using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
using Microsoft.AspNetCore.Cors;

[EnableCors("_myAllowSpecificOrigins")]
[ApiController]
[Route("api/books")]
public class BookController : Controller
{
    private readonly SearchService _searchService;
    private readonly IMediaRepository<Book> _bookRepository;
    private readonly HttpClient _httpClient;
    private readonly string _bookApiKey;

    public BookController(IMediaRepository<Book> bookRepository, IConfiguration configuration, SearchService searchService)
    {
        _bookRepository = bookRepository;
        _searchService = searchService;
        _httpClient = new HttpClient();
        _bookApiKey = configuration["Google:ApiKey"]
                        ?? throw new ArgumentNullException("Invalid API key (Google Books)");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchBooks([FromQuery] string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return BadRequest("Invalid Query");

        try
        {
            var res = await _searchService.SearchBooksAsync(title);
            if (res == null) return NotFound("No books matching search");

            return Ok(res);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error searching books: {e.Message}");
            return StatusCode(500, "An error occurred while searching for books.");
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddBookToUser([FromBody] ItemAddRequest request)
    {
        if (request == null) return BadRequest("Invalid JSON");

        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ItemId))
            return BadRequest("User ID and Book ID are required");

        try
        {
            string url = $"https://www.googleapis.com/books/v1/volumes/{request.ItemId}?key={_bookApiKey}";
            var res = await _httpClient.GetStringAsync(url);

            if (string.IsNullOrWhiteSpace(res))
                return NotFound("Book not found in Google Books API");

            var bookResponse = JsonSerializer.Deserialize<GoogleBook>(res, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (bookResponse?.VolumeInfo == null)
                return NotFound("No book found with this ID");

            string title = bookResponse.VolumeInfo.Title ?? "Unknown Title";
            string authorName = bookResponse.VolumeInfo.Authors != null && bookResponse.VolumeInfo.Authors.Count > 0
                ? string.Join(", ", bookResponse.VolumeInfo.Authors)
                : "Unknown Author";
            string coverUrl = bookResponse.VolumeInfo.ImageLinks?.Thumbnail ?? "";

            var book = new Book
            {
                UserId = request.UserId,
                ApiId = request.ItemId,
                Title = title,
                Authors = authorName,
                Artwork = coverUrl,
                Rating = null
            };

            await _bookRepository.AddItemToUserAsync(request.UserId, book);
            return Ok(new { message = $"{book.Title} added successfully." });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding book: {e.Message}");
            return StatusCode(500, "An error occurred while adding the book.");
        }
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserBooks([FromRoute] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("User ID is required");

        try
        {
            var userBooks = await _bookRepository.GetAllByUserAsync(userId);
            if (userBooks == null || !userBooks.Any())
                return NotFound(new { message = "No books found for user" });

            return Ok(userBooks);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching user books: {e.Message}");
            return StatusCode(500, "An error occurred while retrieving user books.");
        }
    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateBookRating(string userId, string itemId, [FromBody] ItemRatingRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemId))
            return BadRequest("User ID and Item ID cannot be empty");

        if (request.Rating < 0 || request.Rating > 5)
            return BadRequest("Invalid rating value. Rating must be between 0 and 5.");

        try
        {
            bool success = await _bookRepository.UpdateRatingAsync(userId, itemId, request.Rating);
            if (!success) return NotFound("Item not found");

            return Ok("Rating updated successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating book rating: {e.Message}");
            return StatusCode(500, "An error occurred while updating the book rating.");
        }
    }

    [HttpDelete("{userId}/{bookId}")]
    public async Task<ActionResult> DeleteBook(string userId, string bookId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(bookId))
            return BadRequest("User ID and Book ID are required");

        try
        {
            var success = await _bookRepository.DeleteItemAsync(userId, bookId);
            if (!success) return NotFound("Item not found");

            return Ok("Item deleted successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting book: {e.Message}");
            return StatusCode(500, "An error occurred while deleting the book.");
        }
    }
}
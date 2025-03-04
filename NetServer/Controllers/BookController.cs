using System.Runtime.CompilerServices;
using System.Text.Json;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using NetServer.Models;
using NetServer.Repositories;
using ZstdSharp.Unsafe;

[ApiController]
[Route("api/books")]
public class BookController : Controller
{
    private readonly IBookRepository _bookRepository;
    private readonly HttpClient _httpClient;
    private readonly string _bookApiKey;

    public BookController(IBookRepository bookRepository, IConfiguration configuration)
    {
        _bookRepository = bookRepository;
        _httpClient = new HttpClient();
        _bookApiKey = configuration["Google:ApiKey"]
                        ?? throw new ArgumentNullException("Invalid API key (Google Books)");
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddBookToUser([FromBody] ItemAddRequest request)
    {
        if (request == null) return BadRequest("Invalid JSON");

        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.ItemId))
            return BadRequest("User Id and Book Id are required");

        // Use Google Books API
        string url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{request.ItemId}&key={_bookApiKey}";

        var res = await _httpClient.GetStringAsync(url);

        if (string.IsNullOrWhiteSpace(res)) return NotFound("Book not found in Google Books API");

        var bookResponse = JsonSerializer.Deserialize<GoogleBooksResponse>(res, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (bookResponse == null || bookResponse.Items == null || bookResponse.Items.Count == 0)
            return NotFound("No book found with this ISBN");

        var bookInfo = bookResponse.Items[0].VolumeInfo;

        if (bookInfo == null) return BadRequest("Invalid book data received");

        string title = bookInfo.Title ?? "Unknown Title";
        string authorName = (bookInfo.Authors != null && bookInfo.Authors.Count > 0) ? string.Join(", ", bookInfo.Authors) : "Unknown Author";
        string coverUrl = bookInfo.ImageLinks?.Thumbnail ?? "";

        var book = new Book
        {
            UserId = request.UserId,
            ApiId = request.ItemId,
            Title = title,
            Authors = authorName,
            Artwork = coverUrl,
            Rating = null
        };

        await _bookRepository.AddBookAsync(request.UserId, book);
        return Ok($"Book '{book.Title}' added successfully");
    }

    [HttpGet("{userId}/all")]
    public async Task<IActionResult> GetAllUserBooks([FromRoute] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("User Id is requited");

        var userBooks = await _bookRepository.GetAllBooksyUserAsync(userId);

        if (userBooks == null || !userBooks.Any())
            return NotFound(new { message = "No books found for user" });

        return Ok(userBooks);
    }

    [HttpPut("{userId}/rate/{itemId}")]
    public async Task<IActionResult> UpdateBookRating(string userId, string itemId, [FromBody] ItemRatingRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemId))
            return BadRequest("User Id and Item Id cannot be empty");

        if (request.Rating < 0 || request.Rating > 5)
            return BadRequest("Invalid rating value. Rating must be between 0 and 5.");

        bool success = await _bookRepository.UpdateBookRatingAsync(userId, itemId, request.Rating);
        if (!success) return NotFound("Item not found");

        return Ok("Rating updated successfully");
    }

    [HttpDelete("{userId}/{bookId}")]
    public async Task<ActionResult> DeleteBook(string userId, string bookId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(bookId))
            return BadRequest("User ID and Book ID are required");

        var success = await _bookRepository.DeleteBookAsync(userId, bookId);
        if (!success) return NotFound("Item not found");

        return Ok("Item deleted");
    }
}

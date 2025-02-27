namespace NetServer.Models;

public class MovieAddRequest
{
    public required string UserId { get; set; }
    public required string MovieId { get; set; }
}
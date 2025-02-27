namespace NetServer.Models;

public class Music { 
    public required string UserId { get; set; }
    public required string ApiId { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }  
    public required string Artwork { get; set; } 
    public float Rating { get; set; }
}
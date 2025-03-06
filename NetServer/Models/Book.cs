namespace NetServer.Models;

public class Book { 
    public required string UserId { get; set; }
    public required string ApiId { get; set; }
    public required string Title { get; set; }
    public required string Authors { get; set; }
    public required string Artwork { get; set; } 
    public float? Rating { get; set; }
    public List<BookLogEntry> LogEntries { get; set; } = new List<BookLogEntry>(); 
}

public class BookLogEntry { 
    public DateTime DateRead { get; set; }
    // use rating from above    
}
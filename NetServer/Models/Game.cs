namespace NetServer.Models;

public class Game { 
    public required string UserId { get; set; }
    public required string ApiId { get; set; }
    public required string Title { get; set; }
    public required string Artwork { get; set; } 
    public float? Rating { get; set; }
    public List<GameLogEntry> LogEntries { get; set; } = new List<GameLogEntry>(); 
}

public class GameLogEntry { 
    public DateTime DatePlayed { get; set; }
    // use rating from above    
}
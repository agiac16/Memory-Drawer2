namespace NetServer.Models;

public class Movie : IMediaItem { 
    public required string UserId { get; set; }
    public required string ApiId { get; set; }
    public required string Title { get; set; }
    public string? PosterPath { get; set; } 
    public float? Rating { get; set; }   
    public List<MovieLogEntry> LogEntries { get; set; } = new List<MovieLogEntry>(); 
}

public class MovieLogEntry { 
    public DateTime DateWatched { get; set; }
    // use rating from above    
}
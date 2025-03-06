namespace NetServer.Models;

public class Music { 
    public required string UserId { get; set; }
    public required string ApiId { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }  
    public string? Artwork { get; set; } 
    public float? Rating { get; set; }
    public List<MusicLogEntry> LogEntries { get; set; } = new List<MusicLogEntry>(); 
}

public class MusicLogEntry { 
    public DateTime DateListened { get; set; }
    public required string ApiId { get; set; }   
    public float? Rating { get; set; } 
}
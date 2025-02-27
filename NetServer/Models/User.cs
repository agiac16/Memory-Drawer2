using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace NetServer.Models;

public class User
{
    [BsonId] // primary key for mongo
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = ObjectId.GenerateNewId().ToString(); // generate id 

    [Required]
    public required string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;    
    [Required]
    [PasswordValidator]
    public string Password { get; set; } = string.Empty;

    public List<Movie>? Movies { get; set; } = new List<Movie>();
    public List<Music>? Music { get; set; } = new List<Music>();
    public List<Book>? Books { get; set; } = new List<Book>(); 
    public List<Game>? Games { get; set; } = new List<Game>();

    public User() { }
}
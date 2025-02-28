using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetServer.Models;

public class User
{
    [Key] // primary key ef core
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto gens a new id
    public int Id { get; set; }

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
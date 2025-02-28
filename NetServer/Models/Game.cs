using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetServer.Models;

public class Game { 

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // pk
    public int Id { get; set; }

    [Required]
    public string ApiId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Artwork { get; set; } 
    public float? Rating { get; set; }   

    // many to many
    public List<User> Users { get; set; } = new List<User>();
}
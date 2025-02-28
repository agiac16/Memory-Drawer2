using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetServer.Models;

public class Music { 
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string ApiId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Artist { get; set; } = string.Empty;
    public string Artwork { get; set; } = string.Empty;
    public float Rating { get; set; }

    // many to many
    public List<User> Users { get; set; } = new List<User>();
}
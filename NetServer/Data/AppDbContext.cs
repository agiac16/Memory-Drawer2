using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NetServer.Models;

namespace NetServer.Data;

public class AppDbContext : DbContext
{

    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Music> Music { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Book> Books { get; set; }

    // configure to use db
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Many-to-Many: User <-> Movies
        modelBuilder.Entity<User>()
            .HasMany(u => u.Movies)
            .WithMany(m => m.Users)
            .UsingEntity(j => j.ToTable("UserMovies"));  // auto create join

        // Many-to-Many: User <-> Games
        modelBuilder.Entity<User>()
            .HasMany(u => u.Games)
            .WithMany(g => g.Users)
            .UsingEntity(j => j.ToTable("UserGames"));

        // Many-to-Many: User <-> Books
        modelBuilder.Entity<User>()
            .HasMany(u => u.Books)
            .WithMany(b => b.Users)
            .UsingEntity(j => j.ToTable("UserBooks"));

        // Many-to-Many: User <-> Music
        modelBuilder.Entity<User>()
            .HasMany(u => u.Music)
            .WithMany(m => m.Users)
            .UsingEntity(j => j.ToTable("UserMusic"));
    }
}
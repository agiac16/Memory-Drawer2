using NetServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// interface movie operations
namespace NetServer.Repositories;

public interface IMovieRepository { 
    // find by ID
    Task<Movie?> GetByIdAsync(string userId, string movieId);

    // find by title || might use for search user movies feature?
    Task<Movie?> GetByTitleAsync(string userId, string movieTitle); 

    // get all movies
    Task<IEnumerable<Movie>> GetAllByUserAsync(string userId);
    
    // CUD
    Task AddMovieToUserAsync(string userId, Movie movie);
    Task<bool> UpdateMovieAsync(string userId, string movieId, float rating);
    Task<bool> DeleteMovieAsync(string userId, string movieId);
}

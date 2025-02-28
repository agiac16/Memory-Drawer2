using NetServer.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using NetServer.Data;
using Microsoft.EntityFrameworkCore;

namespace NetServer.Repositories {
    public class MovieRepository : IMovieRepository { 
        private readonly AppDbContext _dbContext;
        
        public MovieRepository(AppDbContext dbContext) { 
            _dbContext = dbContext;
        }

        // get all from user
        public async Task<IEnumerable<Movie>> GetAllByUserAsync(string userId) { 
            int userIdInt = int.Parse(userId);
            var user = await _dbContext.Users
                .Include(u => u.Movies)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            return user?.Movies ?? new List<Movie>();
        }

        public async Task<Movie?> GetByIdAsync(string userId, string movieId) {
            int userIdInt = int.Parse(userId);
            var user = await _dbContext.Users
                .Include(u => u.Movies)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            return user?.Movies?.FirstOrDefault(m => m.ApiId == movieId);
        }

        public async Task<Movie?> GetByTitleAsync(string movieTitle, string userId) {
            int userIdInt = int.Parse(userId);
            var user = await _dbContext.Users
                .Include(u => u.Movies)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            return user?.Movies?.FirstOrDefault(m => m.Title == movieTitle);
        }

        // movie passed in from API
        public async Task AddMovieToUserAsync(string userId, Movie movie) {
            if (movie == null) { 
                throw new ArgumentNullException(nameof(movie));
            }

            int userIdInt = int.Parse(userId);
            var user = await _dbContext.Users
                .Include(u => u.Movies)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);
                
            if (user == null) { 
                throw new InvalidOperationException("User not found.");
            }

            // check dupe
            bool exists = user.Movies.Any(m => m.ApiId == movie.ApiId);
            if (exists) throw new InvalidOperationException("Movie already exists in the user's list.");

            // add movie to user collection
            user.Movies.Add(movie);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateMovieAsync(string userId, Movie movie) { 
            if (movie == null) { 
                throw new ArgumentNullException(nameof(movie));
            }

            int userIdInt = int.Parse(userId);
            var user = await _dbContext.Users
                .Include(u => u.Movies)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);
            
            if (user == null) { 
                throw new InvalidOperationException("User not found.");
            }

            // find movie to update
            var movieToUpdate = user.Movies?.FirstOrDefault(m => m.ApiId == movie.ApiId);
            if (movieToUpdate == null) { 
                throw new ArgumentNullException(nameof(movie));
            }

            // update
            movieToUpdate.Title = movie.Title;
            movieToUpdate.PosterPath = movie.PosterPath;
            movieToUpdate.Rating = movie.Rating;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMovieAsync(string userId, string movieId) { 
            if (string.IsNullOrWhiteSpace(userId)) {
                throw new ArgumentNullException(nameof(userId), "User ID is required.");
            }

            if (string.IsNullOrWhiteSpace(movieId)) {
                throw new ArgumentNullException(nameof(movieId), "Movie ID is required.");
            }

            int userIdInt = int.Parse(userId);
            var user = await _dbContext.Users
                .Include(u => u.Movies)
                .FirstOrDefaultAsync(u => u.Id == userIdInt);

            if (user == null) { 
                throw new InvalidOperationException("User not found.");
            }

            var movieToDelete = user.Movies?.FirstOrDefault(m => m.ApiId == movieId);
            if (movieToDelete == null) return false;

            // Remove movie from user's collection
            user!.Movies?.Remove(movieToDelete);
            await _dbContext.SaveChangesAsync();
            return true; 
        }

    }
}
using NetServer.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace NetServer.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMongoCollection<User> _users; // access users collection from db

        public MovieRepository(IMongoClient client)
        {
            var database = client.GetDatabase("mdtwo");
            _users = database.GetCollection<User>("Users");
        }

        // get all from user
        public async Task<IEnumerable<Movie>> GetAllByUserAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            // user may be null.. if not return its movies and an empty list if not
            return user?.Movies ?? new List<Movie>();
        }

        public async Task<Movie?> GetByIdAsync(string userId, string movieId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            return user?.Movies?.Find(m => m.ApiId == movieId);
        }

        public async Task<Movie?> GetByTitleAsync(string movieTitle, string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            return user?.Movies?.Find(m => m.Title == movieTitle);
        }

        // movie passed in from API
        public async Task AddMovieToUserAsync(string userId, Movie movie)
        {
            if (movie == null)
            {
                throw new ArgumentNullException(nameof(movie));
            }

            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // check dupe
            bool exists = user.Movies?.Any(m => m.ApiId == movie.ApiId) ?? false;
            if (exists) throw new InvalidOperationException("Movie already exists in the user's list.");

            user.Movies ??= new List<Movie>(); // Initialize Movies if null
            user.Movies.Add(movie);

            // push to db            
            var update = Builders<User>.Update.Push(u => u.Movies, movie);
            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<bool> UpdateMovieAsync(string userId, string movieId, float rating)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(movieId))
            {
                throw new ArgumentNullException("User ID and Movie ID are required.");
            }

            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // find movie to update
            var movieToUpdate = user.Movies?.Find(m => m.ApiId == movieId);
            if (movieToUpdate == null)
            {
                return false; // Movie not found
            }

            // update only the rating
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.Movies, m => m.ApiId == movieId)
            );

            var update = Builders<User>.Update.Set("Movies.$.Rating", rating);

            var result = await _users.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMovieAsync(string userId, string movieId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID is required.");
            }

            if (string.IsNullOrWhiteSpace(movieId))
            {
                throw new ArgumentNullException(nameof(movieId), "Movie ID is required.");
            }

            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var movieToDelete = user!.Movies?.Find(m => m.ApiId == movieId);
            if (movieToDelete == null) return false;

            // match user and movie
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.Movies, m => m.ApiId == movieId)
            );

            // delete
            var update = Builders<User>.Update.Pull(u => u.Movies, movieToDelete);
            var result = await _users.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0; // true if a movie was deleted

        }

    }
}
using MongoDB.Driver;
using NetServer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetServer.Repositories;

// inherit T from IMediaItem
public class MediaRepository<T> : IMediaRepository<T> where T : class, IMediaItem
{

    private readonly IMongoCollection<User> _users;

    public MediaRepository(IMongoClient client)
    {
        var database = client.GetDatabase("mdtwo");
        _users = database.GetCollection<User>("Users");
    }

    // used often to pull user from db based on id
    private FilterDefinition<User> GetUserFilter(string userId)
    {
        return Builders<User>.Filter.Eq(u => u.Id, userId);
    }

    // needed since music is non singular
    private string GetMongoFieldName() => typeof(T) switch
    {
        Type when typeof(T) == typeof(Movie) => "Movies",
        Type when typeof(T) == typeof(Music) => "Music",
        Type when typeof(T) == typeof(Book) => "Books",
        Type when typeof(T) == typeof(Game) => "Games",
        _ => throw new InvalidOperationException($"Unsupported media type: {typeof(T).Name}")
    };

    public async Task<T?> GetByIdAsync(string userId, string apiId)
    {
        try
        {
            var user = await _users.Find(GetUserFilter(userId)).FirstOrDefaultAsync();

            return user?.GetMediaList<T>()?.FirstOrDefault(m => m.ApiId == apiId);

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching media item by ID: {e.Message}");
            return null;
        }
    }
    public async Task<T?> GetByTitleAsync(string userId, string itemTitle)
    {
        try
        {
            var user = await _users.Find(GetUserFilter(userId)).FirstOrDefaultAsync();
            return user?.GetMediaList<T>().FirstOrDefault(m => m.Title == itemTitle);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching media item by title: {e.Message}");
            return null;
        }

    }

    public async Task<IEnumerable<T>> GetAllByUserAsync(string userId)
{
    try
    {
        var user = await _users.Find(GetUserFilter(userId)).FirstOrDefaultAsync();

        if (user == null) return new List<T>();

        var mediaList = user.GetMediaList<T>() ?? new List<T>();

        // sort items by added
        return mediaList.OrderByDescending(item => (item as IMediaItem)?.AddedAt).ToList();
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error fetching all media items for user: {e.Message}");
        return new List<T>();
    }
}

    public async Task AddItemToUserAsync(string userId, T item)
    {
        try
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var filter = GetUserFilter(userId);
            var user = await _users.Find(filter).FirstOrDefaultAsync();

            if (user == null) throw new InvalidOperationException("User not found");

            // check dupe 
            bool exists = user.GetMediaList<T>().Any(m => m.ApiId == item.ApiId);
            if (exists) throw new InvalidOperationException("Item already exists in user's list");


            var update = Builders<User>.Update.AddToSet(GetMongoFieldName(), item);
            var result = await _users.UpdateOneAsync(filter, update);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding media item to user: {e.Message}");
            return;
        }
    }

    public async Task<bool> UpdateRatingAsync(string userId, string apiId, float rating)
    {
        try
        {
            var mediaField = GetMongoFieldName();

            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var user = await _users.Find(GetUserFilter(userId)).FirstOrDefaultAsync();

            if (user == null) throw new InvalidOperationException("User not found");

            var itemToUpdate = user.GetMediaList<T>().Find(m => m.ApiId == apiId);
            if (itemToUpdate == null) return false;

            var filter = Builders<User>.Filter.And(
                GetUserFilter(userId),
                Builders<User>.Filter.ElemMatch(mediaField, Builders<T>.Filter.Eq(m => m.ApiId, apiId))
            );

            var update = Builders<User>.Update.Set($"{mediaField}.$.Rating", rating);
            var result = await _users.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating rating: {e.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(string userId, string apiId)
    {
        try
        {
            var mediaField = GetMongoFieldName();

            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var user = await _users.Find(GetUserFilter(userId)).FirstOrDefaultAsync();

            if (user == null) throw new InvalidOperationException("User not found");
            var itemToDelete = user.GetMediaList<T>().Find(m => m.ApiId == apiId);
            if (itemToDelete == null) return false;

            var filter = Builders<User>.Filter.And(
                GetUserFilter(userId),
                Builders<User>.Filter.ElemMatch(mediaField, Builders<T>.Filter.Eq(m => m.ApiId, apiId))
            );

            var update = Builders<User>.Update.PullFilter(mediaField, Builders<T>.Filter.Eq(m => m.ApiId, apiId));
            var result = await _users.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting media item: {e.Message}");
            return false;
        }
    }
}

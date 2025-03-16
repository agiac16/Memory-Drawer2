using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using NetServer.Models;
using NetServer.Repositories;
namespace NetServer.Repository;

public class UserRepository : IUserRepository
{
    // import user collection
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoClient client)
    {
        var database = client.GetDatabase("mdtwo");
        _users = database.GetCollection<User>("Users");
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        try
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching user by ID: {e.Message}");
            return null;
        }

    }

    public async Task<User?> GetByEmailOrUsernameAsync(string email, string username)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(username))
            return null; 

        try
        {
            return await _users.Find(u => u.Email == email || u.Username == username).FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching user by email or username: {e.Message}");
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        try
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching user by email: {e.Message}");
            return null;
        }
    }

    public async Task CreateUserAsync(User user)
    {
        if (user == null) return; 

        try
        {
            await _users.InsertOneAsync(user);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating user: {e.Message}");
            return;
        }
    }

    public async Task<bool> UpdateUserAsync(string id, User user)
    {
        if (string.IsNullOrWhiteSpace(id) || user == null) return false;

        try
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == id, user);
            return result.MatchedCount > 0; // found > 0
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating user: {e.Message}");
            return false;
        }

    }

    public async Task<bool> DeleteUserAsync(string id)
    {

        try
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);

            return result.DeletedCount > 0; // found 
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting user: {e.Message}");
            return false;
        }

    }
}
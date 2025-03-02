using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using NetServer.Models;
using NetServer.Repositories;
namespace NetServer.Repository;

public class UserRepository : IUserRepository {
    // import user collection
    private readonly IMongoCollection<User> _users;  

    public UserRepository(IMongoClient client) { 
        var database = client.GetDatabase("mdtwo");
        _users = database.GetCollection<User>("Users");
    }

    public async Task<User?> GetByIdAsync(string id) { 
        if (string.IsNullOrWhiteSpace(id)){ 
            throw new ArgumentNullException(nameof(id));
        }
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }
    
    public async Task<User?> GetByEmailOrUsernameAsync(string email, string username) { 
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(username)) {
            throw new ArgumentNullException("Both email and username cannot be null.");
        }
        return await _users.Find(u => u.Email == email || u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email) { 
        if (string.IsNullOrWhiteSpace(email)) {
            throw new ArgumentNullException(nameof(email));
        }

        return await _users.Find(u => u.Email == email ).FirstOrDefaultAsync();
    }

    public async Task CreateUserAsync(User user) { 
        if (user == null) { 
            throw new ArgumentNullException(nameof(user));
        }
        if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Username)) {
            throw new ArgumentException("Email and Username are required.");
        }
        await _users.InsertOneAsync(user);
    }

    public async Task<bool> UpdateUserAsync(string id, User user) {
        if (string.IsNullOrWhiteSpace(id)) {
            throw new ArgumentNullException(nameof(id));
        }
        if (user == null) {
            throw new ArgumentNullException(nameof(user));
        }

        var result = await _users.ReplaceOneAsync(u => u.Id == id, user);

        return result.MatchedCount > 0; // found > 0
    }

    public async Task<bool> DeleteUserAsync(string id) {
        if (string.IsNullOrWhiteSpace(id)) {
            throw new ArgumentNullException(nameof(id));
        }

        var result = await _users.DeleteOneAsync(u => u.Id == id);
        
        return result.DeletedCount > 0; // found 
    }
}
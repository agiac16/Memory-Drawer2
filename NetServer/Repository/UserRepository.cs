using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using NetServer.Data;
using NetServer.Models;
using NetServer.Repositories;
namespace NetServer.Repository;

public class UserRepository : IUserRepository {
    // import user collection
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext) { 
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(string id) { 
        if (string.IsNullOrWhiteSpace(id)){ 
            throw new ArgumentNullException(nameof(id));
        }
        return await  _dbContext.Users.FindAsync(id);
    }
    
    public async Task<User?> GetByEmailOrUsernameAsync(string email, string username) { 
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(username)) {
            throw new ArgumentNullException("Both email and username cannot be null.");
        }
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email || u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email) { 
        if (string.IsNullOrWhiteSpace(email)) {
            throw new ArgumentNullException(nameof(email));
        }

        return await _dbContext.Users.FindAsync(email);
    }

    public async Task CreateAsync(User user) { 
        if (user == null) { 
            throw new ArgumentNullException(nameof(user));
        }
        if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Username)) {
            throw new ArgumentException("Email and Username are required.");
        }
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(string id, User user) {
        if (string.IsNullOrWhiteSpace(id)) {
            throw new ArgumentNullException(nameof(id));
        }
        if (user == null) {
            throw new ArgumentNullException(nameof(user));
        }

        var exisingUser = await _dbContext.Users.FindAsync(id);
        if (exisingUser == null) { 
            return false;
        }

        exisingUser.Username = user.Username;
        exisingUser.Email = user.Email;
        exisingUser.Password = user.Password; 

        _dbContext.Users.Update(exisingUser);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id) {
        if (string.IsNullOrWhiteSpace(id)) {
            throw new ArgumentNullException(nameof(id));
        }

        var user = await _dbContext.Users.FindAsync(id);
        if (user == null) { 
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
using NetServer.Models;
// interface user operations
namespace NetServer.Repositories;

public interface IUserRepository {
    // task for nonblocking operations
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailOrUsernameAsync(string email, string username);
    Task<User?> GetByEmailAsync(string email);
    Task CreateAsync(User user);
    Task<bool> UpdateAsync(string id, User user);
    Task<bool> DeleteAsync(string id);
}

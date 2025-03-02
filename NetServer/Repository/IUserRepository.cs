using NetServer.Models;
// interface user operations
namespace NetServer.Repositories;

public interface IUserRepository {
    // task for nonblocking operations
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailOrUsernameAsync(string email, string username);
    Task<User?> GetByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(string id, User user);
    Task<bool> DeleteUserAsync(string id);
}

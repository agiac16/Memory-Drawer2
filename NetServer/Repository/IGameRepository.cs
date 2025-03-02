using NetServer.Models;
namespace NetServer.Repositories;
public interface IGameRepository { 
    Task<Game?> GetByGameIdAsync(string userId, string gameId);

    Task<Game?> GetByGameNameAsync(string userId, string gameName);    
    
    Task<IEnumerable<Game>> GetAllGamesByUserAsync(string userId);

    Task AddGameAsync(string userId, Game game);

    Task<bool> UpdateGameRatingAsync(string userId, string gameId, float rating);

    Task<bool> DeleteGameAsync(string userId, string gameId);

}
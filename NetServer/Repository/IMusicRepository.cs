using NetServer.Models;
namespace NetServer.Repositories;
public interface IMusicRepository { 
    Task<Music?> GetByMusicIdAsync(string userId, string musicId);

    Task<Music?> GetByMusicNameAsync(string userId, string musicName);    
    
    Task<IEnumerable<Music>> GetAllMusicByUserAsync(string userId);

    Task AddMusicAsync(string userId, Music music);

    Task<bool> UpdateMusicRatingAsync(string userId, string musicId, float rating);

    Task<bool> DeleteMusicAsync(string userId, string musicId);

}
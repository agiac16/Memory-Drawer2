// unified repo since they all have same functions...

namespace NetServer.Repositories;

public interface IMediaRepository<T> where T : class {
    Task<T?> GetByIdAsync(string userId, string apiId); 
    Task<T?> GetByTitleAsync(string userId, string itemTitle); 
    Task<IEnumerable<T>> GetAllByUserAsync(string userId); 
    Task AddItemToUserAsync(string userId, T item); 
    Task<bool> UpdateRatingAsync(string userId, string apiId, float rating);
    Task<bool>  DeleteItemAsync(string userId, string apiId);
    Task<bool> LogEntryAsync(string userId, string apiId, DateTime dateConsumed, float? rating = null);
}
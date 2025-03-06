using MongoDB.Driver;
using NetServer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetServer.Repositories;

public class MediaRepository<T> : IMediaRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string userId, string apiId){

    }
    Task<T?> GetByTitleAsync(string userId, string itemTitle) {

    }
    Task<IEnumerable<T>> GetAllByUserAsync(string userId) {

    }
    Task AddItemToUserAsync(string userId, T item) {

    }
    Task<bool> UpdateRatingAsync(string userId, string apiId, float rating) {

    }
    Task<bool> DeleteItemAsync(string userId, string apiId) {

    }
    Task<bool> LogEntryAsync(string userId, string apiId, DateTime dateConsumed, float? rating = null) {
        
    }
}

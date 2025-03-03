using System.Net.Http.Headers;
using System.Xml.XPath;
using MongoDB.Driver;
using NetServer.Models;
namespace NetServer.Repositories;
public class GameRepository : IGameRepository { 

    private readonly IMongoCollection<User> _users;

    public GameRepository(MongoClient client) { 
        var database = client.GetDatabase("mdtwo");
        _users = database.GetCollection<User>("Users");
    }

    private FilterDefinition<User> GetUserFilter(string userId) { 
        return Builders<User>.Filter.Eq(u => u.Id, userId);
    }
    
    public async Task<Game?> GetByGameIdAsync(string userId, string gameId) { 
        if (string.IsNullOrWhiteSpace(gameId)) throw new ArgumentNullException(nameof(gameId));

        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId), 
            Builders<User>.Filter.ElemMatch(u => u.Games, g => g.ApiId == gameId)
        );

        var projection = Builders<User>.Projection.Expression(u => 
            u.Games != null ? u.Games.FirstOrDefault(g => g.ApiId == gameId) : null
        );

        return await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
    }

    public async Task<Game?> GetByGameNameAsync(string userId, string gameName) { 
        if (string.IsNullOrWhiteSpace(gameName)) throw new ArgumentNullException(nameof(gameName));
        
        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId), 
            Builders<User>.Filter.ElemMatch(u => u.Games, g => g.Title == gameName)
        );

        // fetches only the game we need... not user obj
        var projection = Builders<User>.Projection.Expression(u => 
            u.Games != null ? u.Games.FirstOrDefault(g => g.Title == gameName) : null // if do first else second
        );
        return await _users.Find(filter).Project(projection).FirstOrDefaultAsync();

    }
    
    public async Task<IEnumerable<Game>> GetAllGamesByUserAsync(string userId) {
       var filter = GetUserFilter(userId); 
       var projection = Builders<User>.Projection.Expression(u => u.Games ?? new List<Game>()); // projection allows for return of only specific field, not entire document

       return await _users.Find(filter).Project(projection).FirstOrDefaultAsync() ?? new List<Game>(); // Find user, applu projection and return, if not a new empty list.
    }

    public async Task AddGameAsync(string userId, Game game) { 
        if (game == null) throw new ArgumentNullException(nameof(game));

        var filter = GetUserFilter(userId);
        // check dupe with set
        var update = Builders<User>.Update.AddToSet(u => u.Games, game); // push game into Games collection

        var result = await _users.UpdateOneAsync(filter, update); 

        if (result.ModifiedCount == 0) 
            throw new InvalidOperationException("Game already exists or invalid user");
    }

    public async Task<bool> UpdateGameRatingAsync(string userId, string gameId, float rating) { 
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

        var filter = Builders<User>.Filter.And( // filter through and check both conditions
            GetUserFilter(userId),
            Builders<User>.Filter.ElemMatch(u => u.Games, g => g.ApiId == gameId) // game exits
        );

        var update = Builders<User>.Update.Set("Games.$.Rating", rating); // first matching element in db update rating
        var result = await _users.UpdateOneAsync(filter, update); // use filter to make sure both exist

        if (result.ModifiedCount == 0) throw new InvalidOperationException("No game to update or rating unchanged.");
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteGameAsync(string userId, string gameId) { 
        if (string.IsNullOrWhiteSpace(gameId)) throw new ArgumentNullException(nameof(gameId));

        // check if user and game exist
        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId), 
            Builders<User>.Filter.ElemMatch(u => u.Games, g => g.ApiId == gameId)
        );

        // search and delete
        var update = Builders<User>.Update.PullFilter(u => u.Games, g => g.ApiId == gameId);
        var result = await _users.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0; 
    }

}
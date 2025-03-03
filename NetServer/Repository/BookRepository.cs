using System.Net.Http.Headers;
using System.Xml.XPath;
using DnsClient.Protocol;
using MongoDB.Driver;
using NetServer.Models;
namespace NetServer.Repositories;
public class BookRepository : IBookRepository { 
    private readonly IMongoCollection<User> _users;

    public BookRepository(MongoClient client) { 
        var database = client.GetDatabase("mdtwo");
        _users = database.GetCollection<User>("Users");
    }

    private FilterDefinition<User> GetUserFilter(string userId) {
        return Builders<User>.Filter.Eq(u => u.Id, userId);
    }
    public async Task<Book?> GetByBookIdAsync(string userId, string bookId){
        if (string.IsNullOrWhiteSpace(bookId)) throw new ArgumentNullException(nameof(bookId));

        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId),
            Builders<User>.Filter.ElemMatch(u => u.Books, b => b.ApiId == bookId)
        );
        
        var projection = Builders<User>.Projection.Expression(u => 
            u.Books != null ? u.Books.FirstOrDefault(b => b.ApiId == bookId) : null
        );

        return await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
    }

    public async Task<Book?> GetByBookNameAsync(string userId, string bookName) {
        if (string.IsNullOrWhiteSpace(bookName)) throw new ArgumentNullException(nameof(bookName));
        
        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId), 
            Builders<User>.Filter.ElemMatch(u => u.Books, b => b.Title == bookName)
        );

        var projection = Builders<User>.Projection.Expression(u => 
            u.Books != null ? u.Books.FirstOrDefault(b => b.Title == bookName) : null
        );

        return await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
    }
    
    public async Task<IEnumerable<Book>> GetAllBooksyUserAsync(string userId) {
        var filter = GetUserFilter(userId);
        var projection = Builders<User>.Projection.Expression(u => u.Books ?? new List<Book>());

        return await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
        
    }

    public async Task AddBookAsync(string userId, Book book) {
        if (book == null) throw new ArgumentNullException(nameof(book));
        
        var filter = GetUserFilter(userId);
        var update = Builders<User>.Update.AddToSet(u => u.Books, book);

        var result = await _users.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
            throw new InvalidOperationException("Book already exists or invalid user");
    }

    public async Task<bool> UpdateBookRatingAsync(string userId, string bookId, float rating) {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId),
            Builders<User>.Filter.ElemMatch(u => u.Books, b => b.ApiId == bookId) 
        );

        var update = Builders<User>.Update.Set("Books.$.Rating", rating); 
        var result = await _users.UpdateOneAsync(filter, update); 

        if (result.ModifiedCount == 0) throw new InvalidOperationException("No book to update or rating unchanged.");
        
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteBookAsync(string userId, string bookId) {
        if (string.IsNullOrWhiteSpace(bookId)) throw new ArgumentNullException(nameof(bookId));

        // check if user and game exist
        var filter = Builders<User>.Filter.And(
            GetUserFilter(userId), 
            Builders<User>.Filter.ElemMatch(u => u.Games, b => b.ApiId == bookId)
        );

        // search and delete
        var update = Builders<User>.Update.PullFilter(u => u.Games, b => b.ApiId == bookId);
        var result = await _users.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0; 
    }
}

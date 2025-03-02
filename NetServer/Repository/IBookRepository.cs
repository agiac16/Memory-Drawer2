using NetServer.Models;
namespace NetServer.Repositories;
public interface IBookRepository { 
    Task<Game?> GetByBookIdAsync(string userId, string bookId);

    Task<Game?> GetByBookNameAsync(string userId, string bookName);    
    
    Task<IEnumerable<Game>> GetAllBooksyUserAsync(string userId);

    Task AddBookAsync(string userId, Book book);

    Task<bool> UpdateBookRatingAsync(string userId, string bookId, float rating);

    Task<bool> DeleteBookAsync(string userId, string bookId);

}
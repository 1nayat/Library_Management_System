using Library_Management_System.Models;
using Library_Management_System.Pagination;

namespace Library_Management_System.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book> GetByIdAsync(int id);
        Task InsertAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);
        Task<IEnumerable<Book>> GetAllAvailableAsync();
        Task<PaginatedList<Book>> GetPaginatedBooksAsync(string search, int page, int pageSize);

        Task<(IEnumerable<Book> Books, int TotalCount)> GetPaginatedAvailableBooksAsync(string? search, int page, int pageSize);

    }
}

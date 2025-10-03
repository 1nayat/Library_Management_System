using Library_Management_System.Models;
using Library_Management_System.Pagination;

namespace Library_Management_System.Repositories
{
    public interface IRentRepository
    {
        Task<IEnumerable<Rent>> GetAllAsync();
        Task<Rent> GetByIdAsync(int id);
        Task InsertAsync(Rent rent);
        Task UpdateAsync(Rent rent);
        Task DeleteAsync(int id);

        Task<IEnumerable<Rent>> GetAllIssuedAsync();
        Task<IEnumerable<Rent>> GetByUserIdAsync(int userId);

        // New method
        Task IssueBookAsync(int userId, int bookId, int days = 14); // default 2 weeks
        Task<IEnumerable<Rent>> GetIssuedBooksByUserIdAsync(int userId);
        Task<PaginatedList<Rent>> GetPaginatedIssuedBooksAsync(string search, int page, int pageSize);
    }
}

using Library_Management_System.Models;
using Library_Management_System.Pagination;

namespace Library_Management_System.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<int> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);        
        Task UpdateAsync(User user);             
        Task DeleteAsync(int id);
        bool VerifyPassword(string hashedPassword, string plainPassword);
        Task<IEnumerable<User>> GetAllStudentsAsync();
        Task<IEnumerable<User>> GetAllAsync();
        Task<PaginatedList<User>> GetPaginatedStudentsAsync(string search, int page, int pageSize);

    }
}

using Library_Management_System.Models;

namespace Library_Management_System.Repositories
{
    public interface IPenaltyRepository
    {
        Task<IEnumerable<Penalty>> GetAllAsync();
        Task<Penalty?> GetByIdAsync(int id);
        Task InsertAsync(Penalty penalty);
        Task UpdateAsync(Penalty penalty);
        Task DeleteAsync(int id);

        Task MarkAsPaidAsync(int penaltyId);
        Task<IEnumerable<Penalty>> GetByUserIdAsync(int userId);

    }
}

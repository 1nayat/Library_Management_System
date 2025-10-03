using Dapper;
using Library_Management_System.Models;
using System.Data;

namespace Library_Management_System.Repositories
{
    public class PenaltyRepository : IPenaltyRepository
    {
        private readonly IDbConnection _db;

        public PenaltyRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<Penalty>> GetAllAsync()
        {
            const string sql = @"
        SELECT 
            p.PenaltyID, 
            p.UserID, 
            p.BookID,
            b.BookName, 
            p.Price, 
            p.PenaltyAmount, 
            p.Detail, 
            p.EntryDate,
            u.Name AS StudentName,
            p.IsPaid
        FROM Penalty4 p
        INNER JOIN Book4 b ON p.BookID = b.BookID
        INNER JOIN Users4 u ON p.UserID = u.Id";

            return await _db.QueryAsync<Penalty>(sql);
        }



        public async Task<Penalty?> GetByIdAsync(int id) =>
            await _db.QuerySingleOrDefaultAsync<Penalty>(
                "SELECT * FROM Penalty4 WHERE PenaltyID=@Id",
                new { Id = id });

        public async Task InsertAsync(Penalty penalty) =>
            await _db.ExecuteAsync(@"
                INSERT INTO Penalty4 (UserID, BookID, BookName, Price, PenaltyAmount, Detail, EntryDate) 
                VALUES (@UserID, @BookID, @BookName, @Price, @PenaltyAmount, @Detail, @EntryDate)",
                penalty);

        public async Task UpdateAsync(Penalty penalty) =>
            await _db.ExecuteAsync(@"
                UPDATE Penalty4 
                SET UserID=@UserID, 
                    BookID=@BookID, 
                    BookName=@BookName, 
                    Price=@Price, 
                    PenaltyAmount=@PenaltyAmount, 
                    Detail=@Detail,
                    EntryDate=@EntryDate
                WHERE PenaltyID=@PenaltyID", penalty);

        public async Task DeleteAsync(int id) =>
            await _db.ExecuteAsync(
                "DELETE FROM Penalty4 WHERE PenaltyID=@Id",
                new { Id = id });

        public async Task MarkAsPaidAsync(int penaltyId)
        {
            const string sql = "UPDATE Penalty4 SET IsPaid = 1 WHERE PenaltyID = @Id";
            await _db.ExecuteAsync(sql, new { Id = penaltyId });
        }
        public async Task<IEnumerable<Penalty>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT 
                    PenaltyID,
                    UserID,
                    BookID,
                    Price,
                    PenaltyAmount,
                    Detail,
                    EntryDate,
                    BookName,
                    IsPaid
                FROM Penalty4
                WHERE UserID = @UserID
                ORDER BY EntryDate DESC";

            return await _db.QueryAsync<Penalty>(sql, new { UserID = userId });
        }

    }


}


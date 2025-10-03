using Dapper;
using Library_Management_System.Models;
using Library_Management_System.Pagination;
using System.Data;

namespace Library_Management_System.Repositories
{
    public class RentRepository : IRentRepository
    {
        private readonly IDbConnection _db;

        public RentRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<Rent>> GetAllAsync() =>
            await _db.QueryAsync<Rent>("SELECT * FROM Rent4");

        public async Task<Rent?> GetByIdAsync(int id) =>
            await _db.QuerySingleOrDefaultAsync<Rent>("SELECT * FROM Rent4 WHERE RentID=@Id", new { Id = id });

        public async Task InsertAsync(Rent rent)
        {
            const string sql = @"
        INSERT INTO Rent4 (BookID, UserID, Days, IssueDate, ReturnDate, Status)
        VALUES (@BookID, @UserID, @Days, @IssueDate, @ReturnDate, @Status)";
            await _db.ExecuteAsync(sql, rent);
        }

        public async Task UpdateAsync(Rent rent) =>
            await _db.ExecuteAsync(
                @"UPDATE Rent4 
                  SET BookID=@BookID, UserID=@UserID, Days=@Days, IssueDate=@IssueDate, ReturnDate=@ReturnDate, Status=@Status 
                  WHERE RentID=@RentID", rent);

        public async Task DeleteAsync(int id) =>
            await _db.ExecuteAsync("DELETE FROM Rent4 WHERE RentID=@Id", new { Id = id });

        public async Task<IEnumerable<Rent>> GetAllIssuedAsync()
        {
            const string sql = @"
        SELECT r.RentID, r.BookID, r.UserID, r.Days, r.IssueDate, r.ReturnDate, r.Status, 
               b.BookName, u.Name AS StudentName
        FROM Rent4 r
        INNER JOIN Book4 b ON r.BookID = b.BookID
        INNER JOIN Users4 u ON r.UserID = u.Id
        WHERE r.Status = 1"; 

            return await _db.QueryAsync<Rent>(sql);
        }

        public async Task<IEnumerable<Rent>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
        SELECT r.RentID, r.BookID, r.UserID, r.Days, r.IssueDate, r.ReturnDate, r.Status, 
               b.BookName
        FROM Rent4 r
        INNER JOIN Book4 b ON r.BookID = b.BookID
        WHERE r.UserID = @UserID";

            return await _db.QueryAsync<Rent>(sql, new { UserID = userId });
        }

      
        public async Task IssueBookAsync(int userId, int bookId, int days = 14)
        {
        
            var available = await _db.QuerySingleOrDefaultAsync<int>(
                "SELECT AvailableCopies FROM Book4 WHERE BookID = @BookID", new { BookID = bookId });

            if (available <= 0)
                throw new InvalidOperationException("No available copies for this book.");

          
            var sqlInsert = @"
                INSERT INTO Rent4 (BookID, UserID, Days, IssueDate, ReturnDate, Status)
                VALUES (@BookID, @UserID, @Days, @IssueDate, @ReturnDate, @Status)";

            await _db.ExecuteAsync(sqlInsert, new
            {
                BookID = bookId,
                UserID = userId,
                Days = days,
                IssueDate = DateTime.UtcNow,
                ReturnDate = DateTime.UtcNow.AddDays(days),
                Status = 1 
            });

           
            await _db.ExecuteAsync(
                "UPDATE Book4 SET AvailableCopies = AvailableCopies - 1 WHERE BookID = @BookID",
                new { BookID = bookId });
        }
        public async Task<IEnumerable<Rent>> GetIssuedBooksByUserIdAsync(int userId)
        {
            const string sql = @"
        SELECT r.RentID, r.BookID, r.UserID, r.Days, r.IssueDate, r.ReturnDate, r.Status,
               b.BookID, b.BookName, b.Author
        FROM Rent4 r
        INNER JOIN Book4 b ON r.BookID = b.BookID
        WHERE r.UserID = @UserID";

            return await _db.QueryAsync<Rent, Book, Rent>(
                sql,
                (rent, book) =>
                {
                    rent.Book = book; 
                    return rent;
                },
                new { UserID = userId },
                splitOn: "BookID"
            );
        }
        public async Task<PaginatedList<Rent>> GetPaginatedIssuedBooksAsync(string search, int page, int pageSize)
        {
            // Base query for issued books
            var sql = @"
                SELECT r.RentID, r.BookID, r.UserID, r.Days, r.IssueDate, r.ReturnDate, r.Status,
                       b.BookName, u.Name AS StudentName
                FROM Rent4 r
                INNER JOIN Book4 b ON r.BookID = b.BookID
                INNER JOIN Users4 u ON r.UserID = u.Id
                WHERE r.Status = 1";

            // Search filter
            if (!string.IsNullOrEmpty(search))
                sql += " AND (b.BookName LIKE @Search OR u.Name LIKE @Search)";

            sql += " ORDER BY r.IssueDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var rents = await _db.QueryAsync<Rent>(
                sql,
                new { Search = $"%{search}%", Offset = (page - 1) * pageSize, PageSize = pageSize });

            // Total count for pagination
            var countSql = @"
                SELECT COUNT(*)
                FROM Rent4 r
                INNER JOIN Book4 b ON r.BookID = b.BookID
                INNER JOIN Users4 u ON r.UserID = u.Id
                WHERE r.Status = 1";

            if (!string.IsNullOrEmpty(search))
                countSql += " AND (b.BookName LIKE @Search OR u.Name LIKE @Search)";

            int totalCount = await _db.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%" });

            return new PaginatedList<Rent>(rents.ToList(), totalCount, page, pageSize);
        }
    }

}



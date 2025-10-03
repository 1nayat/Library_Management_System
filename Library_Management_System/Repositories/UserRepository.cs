using BCrypt.Net;
using Dapper;
using Library_Management_System.Data;
using Library_Management_System.Models;
using Library_Management_System.Pagination;

namespace Library_Management_System.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperDbContext _db;

        public UserRepository(DapperDbContext db) => _db = db;

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = _db.CreateConnection();
            //return db connection obj
            const string sql = "SELECT * FROM [Users4] WHERE Email = @Email";
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = _db.CreateConnection();
            const string sql = "SELECT * FROM [Users4] WHERE Id = @Id";
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(User user)
        {
            if (user.CreatedAt == default)
                user.CreatedAt = DateTime.Now;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            using var conn = _db.CreateConnection();
            const string sql = @"
                INSERT INTO [Users4] (Name, Email, PasswordHash, Role, Image, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Email, @PasswordHash, @Role, @Image, @CreatedAt);
            ";

            return await conn.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task UpdateAsync(User user)
        {
            using var conn = _db.CreateConnection();
            const string sql = @"
                UPDATE [Users4] 
                SET Name = @Name,
                    Email = @Email,
                    PasswordHash = @PasswordHash,
                    Role = @Role,
                    Image = @Image
                WHERE Id = @Id
            ";

            await conn.ExecuteAsync(sql, user);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = _db.CreateConnection();
            const string sql = "DELETE FROM [Users4] WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id });
        }

        public bool VerifyPassword(string hashedPassword, string plainPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
        public async Task<IEnumerable<User>> GetAllStudentsAsync()
        {
            using var conn = _db.CreateConnection();
            const string sql = "SELECT * FROM [Users4] WHERE Role = 'Student'";
            return await conn.QueryAsync<User>(sql);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var conn = _db.CreateConnection();
            const string sql = "SELECT * FROM [Users4]";
            return await conn.QueryAsync<User>(sql);
        }
        /////////////////////
        ///

        public async Task<PaginatedList<User>> GetPaginatedStudentsAsync(string search, int page, int pageSize)
        {
            using var conn = _db.CreateConnection();

            var sql = "SELECT * FROM Users4 WHERE Role = 'Student'";

            if (!string.IsNullOrEmpty(search))
                sql += " AND Name LIKE @Search";

            sql += " ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var students = await conn.QueryAsync<User>(
                sql,
                new { Search = $"%{search}%", Offset = (page - 1) * pageSize, PageSize = pageSize });

            // Total count for pagination
            var countSql = "SELECT COUNT(*) FROM Users4 WHERE Role = 'Student'";
            if (!string.IsNullOrEmpty(search))
                countSql += " AND Name LIKE @Search";

            int totalCount = await conn.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%" });

            return new PaginatedList<User>(students.ToList(), totalCount, page, pageSize);
        }

    }
}

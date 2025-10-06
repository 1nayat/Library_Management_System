using Dapper;
using Library_Management_System.Data;
using Library_Management_System.Models;
using Library_Management_System.Pagination;
using System.Data;

namespace Library_Management_System.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly DapperDbContext _dbContext;

        public BookRepository(DapperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            using var conn = _dbContext.CreateConnection();
            const string sql = "SELECT * FROM Book4";
            return await conn.QueryAsync<Book>(sql);
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            using var conn = _dbContext.CreateConnection();
            const string sql = "SELECT * FROM Book4 WHERE BookID=@Id";
            var book = await conn.QuerySingleOrDefaultAsync<Book>(sql, new { Id = id });
            return book;
        }

        public async Task InsertAsync(Book book)
        {
            using var conn = _dbContext.CreateConnection();

         
            book.Image ??= "";

            const string sql = @"INSERT INTO Book4 
                         (BookName, Author, Detail, Price, Publication, Branch, Quantities, AvailableQnt, RentQnt, Image) 
                         VALUES 
                         (@BookName, @Author, @Detail, @Price, @Publication, @Branch, @Quantities, @AvailableQnt, @RentQnt, @Image)";
            await conn.ExecuteAsync(sql, book);
        }


        public async Task UpdateAsync(Book book)
        {
            using var conn = _dbContext.CreateConnection();

            
            book.Image ??= "";

            const string sql = @"UPDATE Book4 
                         SET BookName=@BookName, Author=@Author, Detail=@Detail, Price=@Price, 
                             Publication=@Publication, Branch=@Branch, Quantities=@Quantities, 
                             AvailableQnt=@AvailableQnt, RentQnt=@RentQnt, Image=@Image
                         WHERE BookID=@BookID";
            await conn.ExecuteAsync(sql, book);
        }


        public async Task DeleteAsync(int id)
        {
            using var conn = _dbContext.CreateConnection();
            const string sql = "DELETE FROM Book4 WHERE BookID=@Id";
            await conn.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Book>> GetAllAvailableAsync()
        {
            using var conn = _dbContext.CreateConnection();
            const string sql = "SELECT * FROM Book4 WHERE AvailableQnt > 0";
            return await conn.QueryAsync<Book>(sql);
        }
        /////////////////////////////////////////////////
        public async Task<PaginatedList<Book>> GetPaginatedBooksAsync(string search = "", int page = 1, int pageSize = 10)
        {
            using var conn = _dbContext.CreateConnection();

            var countSql = @"
                SELECT COUNT(*) FROM Book4
                WHERE (@Search IS NULL OR BookName LIKE '%' + @Search + '%' OR Author LIKE '%' + @Search + '%')";
            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, new { Search = search });

            var sql = @"
                SELECT * FROM Book4
                WHERE (@Search IS NULL OR BookName LIKE '%' + @Search + '%' OR Author LIKE '%' + @Search + '%')
                ORDER BY BookID
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var items = await conn.QueryAsync<Book>(sql, new
            {
                Search = search,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });

            return new PaginatedList<Book>(items, totalCount, page, pageSize);
        }

        public async Task<(IEnumerable<Book> Books, int TotalCount)> GetPaginatedAvailableBooksAsync(string? search, int page, int pageSize)
        {
            using var conn = _dbContext.CreateConnection();

            string countSql = @"
                SELECT COUNT(*) 
                FROM Book4
                WHERE AvailableQnt > 0
                AND (@Search IS NULL OR BookName LIKE '%' + @Search + '%' OR Author LIKE '%' + @Search + '%')";

            string sql = @"
                SELECT * 
                FROM Book4
                WHERE AvailableQnt > 0
                AND (@Search IS NULL OR BookName LIKE '%' + @Search + '%' OR Author LIKE '%' + @Search + '%')
                ORDER BY BookName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, new { Search = search });

            var books = await conn.QueryAsync<Book>(sql, new
            {
                Search = search,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });

            return (books, totalCount);
        }
    }
}

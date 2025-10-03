using System.Data;
using Microsoft.Data.SqlClient; 

namespace Library_Management_System.Data
{
    public class DapperDbContext
    {
        private readonly IConfiguration _config;
        public string ConnectionString => _config.GetConnectionString("DefaultConnection")!;

        public DapperDbContext(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
    }

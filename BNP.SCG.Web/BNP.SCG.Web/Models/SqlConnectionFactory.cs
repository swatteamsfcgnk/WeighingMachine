using System.Data;
using System.Data.SqlClient;
namespace BNP.SCG.Web.Models
{
    public interface IDatabaseSqlConnectionFactory
    {
        Task<IDbConnection> CreateSqlConnectionAsync();
    }


    public class SqlConnectionFactory : IDatabaseSqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString) => _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public async Task<IDbConnection> CreateSqlConnectionAsync()
        {
            var sqlConnection = new SqlConnection(_connectionString);
            await sqlConnection.OpenAsync();
            return sqlConnection;
        }
    }
}

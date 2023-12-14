using BNP.SCG.Web.Models;
using Dapper;

namespace BNP.SCG.Web.Services
{
    public class InfoService
    {
        IDatabaseSqlConnectionFactory _db;

        public InfoService(IDatabaseSqlConnectionFactory _database, IConfiguration config)
        {
            _db = _database;
        }

        public async Task<string> GetRegisterMessage()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT register_message FROM tb_system_config";
            var message = conn.QuerySingleOrDefault<string>(sql);
            return message;
        }

        public async Task<string> GetWeightInMessage()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT weight_in_message FROM tb_system_config";
            var message = conn.QuerySingleOrDefault<string>(sql);
            return message;
        }

        public async Task<string> GetWeightOutMessage()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT weight_out_message FROM tb_system_config";
            var message = conn.QuerySingleOrDefault<string>(sql);
            return message;
        }

        public async Task<string> GetPercentageMessage()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT over_percentage FROM tb_system_config";
            var message = conn.QuerySingleOrDefault<string>(sql);
            return message;
        }
    }
}

using BNP.SCG.Web.Models;
using Dapper;
using System.Globalization;

namespace BNP.SCG.Web.Services
{
    public class SystemConfigService
    {
        IDatabaseSqlConnectionFactory _db;

        public SystemConfigService(IDatabaseSqlConnectionFactory _database)
        {
            _db = _database;
        }

        public async Task<ReturnObject<long>> EditSystemConfig(SystemConfig model)
        {
            var result = new ReturnObject<long>();

            try
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                var sql = $@"
                    UPDATE tb_system_config
                    SET
                        [register_message] = '{model.register_message}'
                        ,[weight_in_message] = '{model.weight_in_message}'
                        ,[weight_out_message] = '{model.weight_out_message}'                        
                        ,[over_percentage] = '{model.over_percentage}'
                        ,[updated_by] = '{model.updated_by}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'";

                await conn.ExecuteAsync(sql, model, transaction: transaction);
                transaction.Commit();

                result.isCompleted = true;
                result.message.Add("อัพเดทข้อมูลสำเร็จ!");
            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<SystemConfig> GetSystemConfig()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT * FROM tb_system_config";
            var systemConfig = conn.QuerySingleOrDefault<SystemConfig>(sql);
            return systemConfig;
        }

    }

}
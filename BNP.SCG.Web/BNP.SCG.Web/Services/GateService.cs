using BNP.SCG.Web.Models;
using Dapper;
using System.Globalization;

namespace BNP.SCG.Web.Services
{
    public class GateService
    {
        IDatabaseSqlConnectionFactory _db;

        public GateService(IDatabaseSqlConnectionFactory _database, IConfiguration config)
        {
            _db = _database;
        }

        public async Task<ReturnObject<long>> OpenGate(Gate model)
        {
            var result = new ReturnObject<long>();
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                var sql = $@"UPDATE gate_control SET {model.gate_name} = 1";
                // var sql = $@"UPDATE gate_control SET is_open = 1 WHERE gate_name = '{model.gate_name}'";
                await conn.ExecuteAsync(sql, transaction: transaction);

                sql = $@"
                    INSERT INTO tb_gate_detail (
                        [gate_name]
                        ,[status]
                        ,[process]                        
                        ,[fulfill_id]                        
                        ,[customer_id]
                        ,[created_at]
                    ) VALUES (
                        '{model.gate_name}',
                        'open',
                        '{model.process}',                        
                        '{model.fulfill_id}',                     
                        '{model.customer_id}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";
                await conn.ExecuteAsync(sql, transaction: transaction);

                transaction.Commit();

                result.isCompleted = true;
                result.message.Add($"{model.gate_name} is open.");
            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> CloseGate(Gate model)
        {
            var result = new ReturnObject<long>();
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                var sql = $@"UPDATE gate_control SET {model.gate_name} = 0";
                // var sql = $@"UPDATE gate_control SET is_open = 0 WHERE gate_name = '{model.gate_name}'";
                await conn.ExecuteAsync(sql, transaction: transaction);

                sql = $@"
                    INSERT INTO tb_gate_detail (
                        [gate_name]
                        ,[status]
                        ,[process]                        
                        ,[fulfill_id]                        
                        ,[customer_id]
                        ,[created_at]
                    ) VALUES (
                        '{model.gate_name}',
                        'close',
                        '{model.process}',                        
                        '{model.fulfill_id}',                     
                        '{model.customer_id}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";
                await conn.ExecuteAsync(sql, transaction: transaction);

                transaction.Commit();

                result.isCompleted = true;
                result.message.Add($"{model.gate_name} is closed.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

    }
}

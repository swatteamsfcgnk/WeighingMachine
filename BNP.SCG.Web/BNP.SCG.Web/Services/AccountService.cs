using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BNP.SCG.Web.Models;
using Dapper;
using NETCore.Encrypt;

namespace BNP.SCG.Web.Services
{
    public class AccountService
    {
        IDatabaseSqlConnectionFactory _db;
        private readonly string? _myKey;

        public AccountService(IDatabaseSqlConnectionFactory _database, IConfiguration config)
        {
            _db = _database;
            _myKey = config["MyKey"];
        }

        public async Task<ReturnObject<long>> CreateUser(User model)
        {
            var result = new ReturnObject<long>();

            if (IsUsernameExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อผู้ใช้งานถูกใช้แล้ว!");
            }
            else
            {
                try
                {
                    model.password = EncryptProvider.HMACSHA256(model.password, _myKey);
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    INSERT INTO [dbo].[tb_user](
                        [username]
                        ,[password]
                        ,[first_name]
                        ,[last_name]
                        ,[email]
                        ,[role]
                        ,[is_active]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        '{model.username}',
                        '{model.password}',
                        '{model.first_name}',
                        '{model.last_name}',
                        '{model.email}',
                        '{model.role}',
                        '{model.is_active}',
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("เพิ่มผู้ใช้งานสำเร็จ!");

                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditUser(User model)
        {
            var result = new ReturnObject<long>();
            try
            {
                var updated_pass = "";
                if (model.password != null)
                {
                    model.password = EncryptProvider.HMACSHA256(model.password, _myKey);
                    updated_pass = $"[password] = '{model.password}',";
                }

                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                var sql = $@"
                    UPDATE [dbo].[tb_user]
                    SET
                        {updated_pass}
                        [first_name] = '{model.first_name}'
                        ,[last_name] = '{model.last_name}'
                        ,[email] = '{model.email}'
                        ,[role] = '{model.role}'
                        ,[is_active] = '{model.is_active}'
                        ,[updated_by] = '{model.updated_by}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = '{model.id}'";

                await conn.ExecuteAsync(sql, model, transaction: transaction);
                transaction.Commit();

                result.isCompleted = true;
                result.message.Add("แก้ไขข้อมูลผู้ใช้งานสำเร็จ!");
                result.message.Add("ถ้ามีการเปลี่ยนแปลงข้อมูลผู้ใช้งาน กรุณาเข้าสู่ระบบใหม่เพื่อปรับปรุงข้อมูลก่อนการใช้งาน");
            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteUser(int id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM [dbo].[tb_user] WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบผู้ใช้งานสำเร็จ!");

            return result;
        }

        public async Task<ReturnObject<long>> IsUsernameExists(User model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_user] WHERE username = '{model.username}'";
            var user = conn.QuerySingleOrDefault<User>(sql);

            var result = new ReturnObject<long>();
            result.isCompleted = user != null ? true : false;
            return result;
        }

        public async Task<User> GetUserById(int id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_user] WHERE id = {id}";
            var user = conn.QuerySingleOrDefault<User>(sql);
            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT * FROM [dbo].[tb_user]";
            var users = conn.Query<User>(sql).ToList();
            return users;
        }

        public async Task<User> Login(User model)
        {
            model.password = EncryptProvider.HMACSHA256(model.password, _myKey);
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_user] WHERE username = '{model.username}' AND password = '{model.password}'";
            var user = conn.QuerySingleOrDefault<User>(sql);
            return user;
        }

        internal object GetUserById(int? user_id)
        {
            throw new NotImplementedException();
        }
    }


}
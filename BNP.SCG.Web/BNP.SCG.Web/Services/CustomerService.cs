using BNP.SCG.Web.Models;
using Dapper;
using System.Globalization;

namespace BNP.SCG.Web.Services
{
    public class CustomerService
    {
        IDatabaseSqlConnectionFactory _db;

        public CustomerService(IDatabaseSqlConnectionFactory _database, IConfiguration config)
        {
            _db = _database;
        }

        public async Task<ReturnObject<long>> CreateCustomer(Customer model)
        {
            var result = new ReturnObject<long>();

            try
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                var sql = $@"
                    INSERT INTO tb_customer(
                        [name]
                        ,[car_license]
                        ,[phone]                        
                        ,[uniq_data]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]
                        ,[updated_at]
                    ) VALUES (
                        '{model.name}',
                        '{model.car_license}',
                        '{model.phone}',                        
                        '{model.uniq_data}',
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";

                await conn.ExecuteAsync(sql, transaction: transaction);

                long HeaderId = Convert.ToInt64(conn.ExecuteScalar<object>("SELECT @@IDENTITY", null, transaction: transaction));

                transaction.Commit();
                result.data = HeaderId;

                result.isCompleted = true;
                result.message.Add("ลงทะเบียนสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditCustomer(Customer model)
        {
            var result = new ReturnObject<long>();
            try
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                var sql = $@"
                    UPDATE tb_customer
                    SET
                        [name] = '{model.name}'
                        ,[car_license] = '{model.car_license}'
                        ,[phone] = '{model.phone}'
                        ,[updated_by] = '{model.updated_by}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = {model.id}";

                await conn.ExecuteAsync(sql, transaction: transaction);
                transaction.Commit();

                result.isCompleted = true;
                result.message.Add("แก้ไขข้อมูลสำเร็จ!");
            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteCustomer(int id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM tb_customer WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบลูกค้าสำเร็จ!");

            return result;
        }

        public async Task<Customer> GetCustomerById(int id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM tb_customer WHERE id = {id}";
            var Customer = conn.QuerySingleOrDefault<Customer>(sql);
            return Customer;
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT * FROM tb_customer";
            var Customers = conn.Query<Customer>(sql).ToList();
            return Customers;
        }

        public async Task<Customer> GetDataByKeyword(string keyword)
        {
            using var conn = await _db.CreateSqlConnectionAsync();

            //var data = conn.Query<Customer>(_x).FirstOrDefault();
            //return data;


            try
            {
                string _x = $@"SELECT * FROM tb_customer
                WHERE name = '{keyword}'
                OR car_license = '{keyword}'
                OR phone = '{keyword}'
                OR uniq_data = CONVERT(uniqueidentifier, '{keyword}')
                AND CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)";
                var data = conn.Query<Customer>(_x).FirstOrDefault();
                return data;
            }
            catch
            {
                string _x = $@"SELECT * FROM tb_customer
                WHERE name = '{keyword}'
                OR car_license = '{keyword}'
                OR phone = '{keyword}'
                AND CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)";
                var data = conn.Query<Customer>(_x).FirstOrDefault();
                return data;
            }
        }

        public async Task<Customer> GetDataByQR(string qr)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string _x = $@"SELECT * FROM tb_customer
                        WHERE uniq_data LIKE '{qr}'
                        AND CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)";
            var data = conn.Query<Customer>(_x).FirstOrDefault();
            return data;
        }

        public async Task<ReturnObject<long>> Printing(string qr)
        {
            var result = new ReturnObject<long>();
            try
            {
                string[] _data = qr.Split('|');
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();

                string query = @"INSERT INTO [dbo].[tb_print]
                   (print_data_qr
                   ,car_license        
                   ,customer_name
                   ,is_print)
                VALUES
                   (@print_data_qr
                   ,@car_license               
                   ,@customer_name
                   ,0)";

                await conn.ExecuteAsync(query, new { print_data_qr = _data[0], car_license = _data[1], customer_name = _data[2] });

                result.isCompleted = true;
                result.message.Add("Save Success!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

    }
}

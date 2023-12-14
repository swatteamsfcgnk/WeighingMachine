using BNP.SCG.Web.Models;
using Dapper;

namespace BNP.SCG.Web.Services
{
    public class ROPService
    {
        IDatabaseSqlConnectionFactory _db;
        public ROPService(IDatabaseSqlConnectionFactory _database)
        {
            _db = _database;
        }

        public async Task<ReturnObject<long>> CreateROP(ROPData model)
        {
            var result = new ReturnObject<long>();

            if (IsROPExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("วัตถุดิบ และสถานที่นี้มี ROP ถูกตั้งค่าแล้วในระบบ!");
            }
            else
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();
                try
                {
                    var sql = $@"
                          INSERT INTO [dbo].[mp_rop](
                        [material_id]
                        ,[location_id]
                        ,[rop]
                        ,[usge_qty]
                        ,[max_qty]
                        ,[shift]
                        ,[remark]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]
                        ,[updated_at])
                    VALUES
                        (@material_id
                        ,@location_id
                        ,@rop
                        ,@usge_qty
                        ,@max_qty
                        ,@shift
                        ,@remark
                        ,@created_by
                        ,@created_at
                        ,@updated_by
                        ,@updated_at)";
                    await conn.ExecuteAsync(sql, model, transaction: transaction);

                    sql = $@"SELECT 
                        tb_material_type.is_rop
                    FROM tb_material
                        INNER JOIN tb_material_type ON tb_material.raw_material_type_id = tb_material_type.id
                        WHERE tb_material.id = {model.material_id}";
                    bool isRop = conn.ExecuteScalar<bool>(sql, transaction: transaction);

                    if (isRop)
                    {
                        int rop_id = Convert.ToInt32(conn.ExecuteScalar<object>("SELECT @@IDENTITY", null, transaction: transaction));
                        Silo silo = new Silo()
                        {
                            time = model.created_at,
                            location_id = model.location_id,
                            material_id = model.material_id,
                            value = null,
                            unit = "Ton",
                            rop_id = rop_id
                        };
                        sql = $@"INSERT INTO [dbo].[tb_silo]
                           ([time]
                           ,[rop_id]
                           ,[location_id]
                           ,[material_id]
                           ,[value]
                           ,[unit])
                     VALUES
                           (@time
                           ,@rop_id
                           ,@location_id
                           ,@material_id
                           ,@value
                           ,@unit)";
                        await conn.ExecuteAsync(sql, silo, transaction: transaction);
                    }

                    transaction.Commit();
                    result.isCompleted = true;
                    result.message.Add("เพิ่ม ROP สำเร็จ!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditROP(ROPData model)
        {
            var result = new ReturnObject<long>();

            if (IsROPExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("วัตถุดิบ และสถานที่นี้มี ROP ถูกตั้งค่าแล้วในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    UPDATE [dbo].[mp_rop]
                    SET 
                        [rop] = @rop
                        ,[usge_qty] = @usge_qty
                        ,[max_qty] = @max_qty
                        ,[shift] = @shift
                        ,[remark] = @remark
                        ,[updated_by] = @updated_by
                        ,[updated_at] = @updated_at
                    WHERE id = @id";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("แก้ไขข้อมูล ROP สำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteROP(long id)
        {
            var result = new ReturnObject<long>();
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                var sql = $"DELETE FROM [dbo].[mp_rop] WHERE id = '{id}'";
                await conn.ExecuteAsync(sql, transaction: transaction);

                sql = $"DELETE FROM [dbo].[mp_rop_addon] WHERE rop_id = '{id}'";
                await conn.ExecuteAsync(sql, transaction: transaction);

                sql = $"DELETE FROM [dbo].[tb_silo] WHERE rop_id = '{id}'";
                await conn.ExecuteAsync(sql, transaction: transaction);

                transaction.Commit();
                result.isCompleted = true;
                result.message.Add("ลบ ROP สำเร็จ!");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> DeleteROPByMatId(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var sql = $"SELECT id FROM [dbo].[mp_rop] WHERE material_id = '{id}'";
            var rop_id = conn.ExecuteScalar<long>(sql, transaction: transaction);

            return await DeleteROP(rop_id);
        }

        public async Task<ReturnObject<long>> IsROPExists(ROPData model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[mp_rop] WHERE material_id = @material_id AND location_id = @location_id AND id != @id";
            var rop = conn.QuerySingleOrDefault<ROPData>(sql, model);

            var result = new ReturnObject<long>();
            result.isCompleted = rop != null ? true : false;
            return result;
        }

        public async Task<ROPData> GetROPById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT mp_rop.id,
                mp_rop.material_id,
                mp_rop.location_id,
                tb_location.name AS location_name,
                mp_rop.rop,
                mp_rop.usge_qty,
                mp_rop.max_qty,
                mp_rop.shift,
                mp_rop.remark,
                mp_rop.created_by,
                mp_rop.created_at,
                mp_rop.updated_by,
                mp_rop.updated_at, 
                tb_material.raw_material_type_id, 
                tb_material_type.is_rop
                FROM mp_rop
                INNER JOIN tb_material ON mp_rop.material_id = tb_material.id
                INNER JOIN tb_material_type ON tb_material.raw_material_type_id = tb_material_type.id
                INNER JOIN tb_location ON mp_rop.location_id = tb_location.id
                WHERE mp_rop.id = @id";
            var data = conn.QuerySingleOrDefault<ROPData>(sql, new { id });

            return data;
        }

        public async Task<List<ROPData>> GetAllROP()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT mp_rop.id, mp_rop.material_id, tb_material.name AS material_name, mp_rop.location_id, tb_location.name AS location_name, mp_rop.rop, mp_rop.usge_qty, mp_rop.max_qty, mp_rop.shift, mp_rop.remark, mp_rop.created_by, mp_rop.created_at, mp_rop.updated_by, mp_rop.updated_at, tb_material.raw_material_type_id, tb_material_type.is_rop
            FROM mp_rop
            INNER JOIN tb_material ON mp_rop.material_id = tb_material.id
            INNER JOIN tb_material_type ON tb_material_type.id = tb_material.raw_material_type_id
            INNER JOIN tb_location ON mp_rop.location_id = tb_location.id
            ORDER BY id";
            var data = conn.Query<ROPData>(sql).ToList();
            return data;
        }

        public async Task<List<ROPAddOnData>> LoadAddOnROP(long id)
        {
            var result = new List<ROPAddOnData>();
            using var conn = await _db.CreateSqlConnectionAsync();
            var sql = $"SELECT * FROM [dbo].[mp_rop_addon] WHERE rop_id = @id";
            result = conn.Query<ROPAddOnData>(sql, new { id }).ToList();
            return result;
        }

        public async Task<ReturnObject<long>> CreateEditROPAddon(List<ROPAddOnData> model)
        {
            var result = new ReturnObject<long>();
            try
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();
                string sql = @"DELETE FROM [dbo].[mp_rop_addon] WHERE (rop_id = @rop_id)";
                await conn.ExecuteAsync(sql, model, transaction: transaction);

                sql = $@"
                  INSERT INTO [dbo].[mp_rop_addon]
           ([rop_id]
           ,[location_id]
           ,[created_by]
           ,[created_at]
       )
     VALUES
           (@rop_id
           ,@location_id
           ,@created_by
           ,@created_at)";
                foreach (var item in model)
                {
                    await conn.ExecuteAsync(sql, item, transaction: transaction);
                }
                transaction.Commit();
                result.isCompleted = true;
                result.message.Add("เพิ่มสถานที่ลงวัตถุดิบสำเร็จ!");
            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteROPAddon(List<ROPAddOnData> model)
        {
            var result = new ReturnObject<long>();
            try
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();
                string sql = @"DELETE FROM [dbo].[mp_rop_addon] WHERE (rop_id = @rop_id)";
                await conn.ExecuteAsync(sql, model, transaction: transaction);

                transaction.Commit();
                result.isCompleted = true;
                result.message.Add("ลบสถานที่ลงวัตถุดิบสำเร็จ!");
            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }
            return result;
        }

        public async Task<ROPData> GetROPByRawMaterialId(long rm_id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT * FROM [dbo].[mp_rop] WHERE material_id = @rm_id";
            var data = conn.QuerySingleOrDefault<ROPData>(sql, new { rm_id });
            return data;
        }

    }
}
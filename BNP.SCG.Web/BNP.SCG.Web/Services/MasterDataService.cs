using BNP.SCG.Web.Models;
using Dapper;
using System.Globalization;

namespace BNP.SCG.Web.Services
{
    public class MasterDataService
    {
        IDatabaseSqlConnectionFactory _db;
        public MasterDataService(IDatabaseSqlConnectionFactory _database)
        {
            _db = _database;
        }

        public async Task<ReturnObject<long>> CreateRawMaterial(RawMaterial model)
        {
            var result = new ReturnObject<long>();

            if (IsRawMaterialExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อวัตถุดิบซ้ำในระบบ!");
            }
            else
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var trans = conn.BeginTransaction();

                try
                {
                    var sql = "";
                    sql = $@"
                    INSERT INTO [dbo].[tb_material] (
                        [name]
                        ,[category_id]
                        ,[raw_material_type_id]
                        ,[is_active]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        '{model.name}',
                        {model.category_id},
                        {model.raw_material_type_id},
                        '{true}',
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";
                    await conn.ExecuteAsync(sql, model, transaction: trans);

                    long material_id = Convert.ToInt64(conn.ExecuteScalar<object>("SELECT @@IDENTITY", null, transaction: trans));
                    for (int i = 0; i < model.vendors_id.Count; i++)
                    {
                        sql = $@"
                    INSERT INTO [dbo].[tb_material_vendor_list_id] (
                        [material_id]
                        ,[vendor_id]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        {material_id},
                        {model.vendors_id[i]},
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";
                        await conn.ExecuteAsync(sql, model, transaction: trans);
                    }

                    trans.Commit();
                    result.isCompleted = true;
                    result.message.Add("เพิ่มวัตถุดิบสำเร็จ!");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditRawMaterial(RawMaterial model)
        {
            var result = new ReturnObject<long>();

            if (IsRawMaterialExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อวัตถุดิบซ้ำในระบบ!");
            }
            else
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                using var trans = conn.BeginTransaction();
                try
                {
                    var sql = "";

                    sql = $@"
                    UPDATE [dbo].[tb_material]
                    SET
                        [name] = '{model.name}'
                        ,[category_id] = '{model.category_id}'
                        ,[raw_material_type_id] = '{model.raw_material_type_id}'
                        ,[updated_by] = '{model.updated_by}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = '{model.id}'";
                    await conn.ExecuteAsync(sql, model, transaction: trans);

                    sql = $"DELETE FROM [dbo].[tb_material_vendor_list_id] WHERE material_id = {model.id}";
                    await conn.ExecuteAsync(sql, transaction: trans);

                    for (int i = 0; i < model.vendors_id.Count; i++)
                    {
                        sql = $@"
                        INSERT INTO [dbo].[tb_material_vendor_list_id] (
                            [material_id]
                            ,[vendor_id]
                            ,[created_by]
                            ,[created_at]
                            ,[updated_by]    
                            ,[updated_at]
                        ) VALUES (
                            {model.id},
                            {model.vendors_id[i]},
                            '{model.created_by}',
                            '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                            '{model.updated_by}',
                            '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                        )";
                        await conn.ExecuteAsync(sql, model, transaction: trans);
                    }

                    trans.Commit();
                    result.isCompleted = true;
                    result.message.Add("แก้ไขข้อมูลวัตถุดิบสำเร็จ!");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteRawMaterial(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM [dbo].[tb_material] WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบวัตถุดิบสำเร็จ!");

            return result;
        }

        public async Task<ReturnObject<long>> IsRawMaterialExists(RawMaterial model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_material] WHERE name = '{model.name}' AND id != {model.id}";
            var RawMaterial = conn.QuerySingleOrDefault<RawMaterial>(sql);

            var result = new ReturnObject<long>();
            result.isCompleted = RawMaterial != null ? true : false;
            return result;
        }

        public async Task<RawMaterial> GetRawMaterialByName(string material_name)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_material] WHERE name = '{material_name}'";
            var RawMaterial = conn.QuerySingleOrDefault<RawMaterial>(sql);

            return RawMaterial;
        }

        public async Task<RawMaterial> GetRawMaterialById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT t1.*, t2.name as category_name, t3.name as raw_material_type_name 
                            FROM [dbo].[tb_material] as t1
                            LEFT JOIN tb_category as t2 on t2.id = t1.category_id
                            LEFT JOIN tb_material_type as t3 on t3.id = t1.raw_material_type_id
                            WHERE t1.id = {id}";
            var RawMaterial = conn.QuerySingleOrDefault<RawMaterial>(sql);

            sql = $@"SELECT vendor_id FROM [dbo].[tb_material_vendor_list_id]
                    WHERE material_id = {RawMaterial.id}";
            RawMaterial.vendors_id = conn.Query<int>(sql).ToList();

            return RawMaterial;
        }

        public async Task<List<RawMaterial>> GetAllRawMaterials()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT t1.*, t2.name as category_name, t3.name as raw_material_type_name, t3.is_rop 
                            FROM [dbo].[tb_material] as t1
                            LEFT JOIN tb_category as t2 on t2.id = t1.category_id
                            LEFT JOIN tb_material_type as t3 on t3.id = t1.raw_material_type_id";
            var RawMaterials = conn.Query<RawMaterial>(sql).ToList();
            return RawMaterials;
        }

        public async Task<ReturnObject<long>> CreateRawMaterialType(RawMaterialType model)
        {
            var result = new ReturnObject<long>();

            if (IsRawMaterialTypeExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อประเภทวัตถุดิบซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    INSERT INTO [dbo].[tb_material_type](
                        [name], [is_rop]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        '{model.name}',
                        @is_rop,
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("เพิ่มประเภทวัตถุดิบสำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditRawMaterialType(RawMaterialType model)
        {
            var result = new ReturnObject<long>();

            if (IsRawMaterialTypeExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อประเภทวัตถุดิบซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    UPDATE [dbo].[tb_material_type]
                    SET
                        [name] = '{model.name}',     [is_rop] = @is_rop
                        ,[updated_by] = '{model.updated_by}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = '{model.id}'";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("แก้ไขข้อมูลประเภทวัตถุดิบสำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteRawMaterialType(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM [dbo].[tb_material_type] WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบประเภทวัตถุดิบสำเร็จ!");

            return result;
        }

        public async Task<ReturnObject<long>> IsRawMaterialTypeExists(RawMaterialType model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_material_type] WHERE name = '{model.name}' AND id != {model.id}";
            var RawMaterialType = conn.QuerySingleOrDefault<RawMaterialType>(sql);

            var result = new ReturnObject<long>();
            result.isCompleted = RawMaterialType != null ? true : false;
            return result;
        }

        public async Task<RawMaterialType> GetRawMaterialTypeById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_material_type] WHERE id = {id}";
            var RawMaterialType = conn.QuerySingleOrDefault<RawMaterialType>(sql);
            return RawMaterialType;
        }

        public async Task<List<RawMaterialType>> GetAllRawMaterialTypes()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT * FROM [dbo].[tb_material_type]";
            var RawMaterialTypes = conn.Query<RawMaterialType>(sql).ToList();
            return RawMaterialTypes;
        }


        public async Task<ReturnObject<long>> CreateVendor(Vendor model)
        {
            var result = new ReturnObject<long>();

            if (IsVendorExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อคู่ค้าซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    INSERT INTO [dbo].[tb_vendor](
                        [code]
                        ,[name]
                        ,[is_active]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        (SELECT Right('000000' + CONVERT(NVARCHAR, IDENT_CURRENT('tb_vendor')), 6)),
                        '{model.name}',
                        '{true}',
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("เพิ่มคู่ค้าสำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditVendor(Vendor model)
        {
            var result = new ReturnObject<long>();

            if (IsVendorExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อคู่ค้าซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    UPDATE [dbo].[tb_vendor]
                    SET
                        [name] = '{model.name}'
                        ,[updated_by] = '{model.updated_by}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = '{model.id}'";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("แก้ไขข้อมูลคู่ค้าสำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteVendor(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM [dbo].[tb_vendor] WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบคู่ค้าสำเร็จ!");

            return result;
        }

        public async Task<ReturnObject<long>> IsVendorExists(Vendor model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_vendor] WHERE name = '{model.name}' AND id != {model.id}";
            var vendor = conn.QuerySingleOrDefault<Vendor>(sql);

            var result = new ReturnObject<long>();
            result.isCompleted = vendor != null ? true : false;
            return result;
        }

        public async Task<Vendor> GetVendorById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_vendor] WHERE id = {id}";
            var vendor = conn.QuerySingleOrDefault<Vendor>(sql);
            return vendor;
        }

        public async Task<List<Vendor>> GetAllVendors()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT * FROM [dbo].[tb_vendor]";
            var vendors = conn.Query<Vendor>(sql).ToList();
            return vendors;
        }


        public async Task<ReturnObject<long>> CreateLocation(Location model)
        {
            var result = new ReturnObject<long>();

            if (IsLocationExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อสถานที่ซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    INSERT INTO [dbo].[tb_location](
                        [name]
                        ,[is_active]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        '{model.name}',
                        '{true}',
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("เพิ่มสถานที่สำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditLocation(Location model)
        {
            var result = new ReturnObject<long>();

            if (IsLocationExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อสถานที่ซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    UPDATE [dbo].[tb_location]
                    SET
                        [name] = '{model.name}'
                        ,[updated_by] = '{model.updated_by}'
                        ,[Status_sw] = '{model.Status_sw}'
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = '{model.id}'";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("แก้ไขข้อมูลสถานที่สำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteLocation(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM [dbo].[tb_location] WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบสถานที่สำเร็จ!");

            return result;
        }

        public async Task<ReturnObject<long>> IsLocationExists(Location model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_location] WHERE name = '{model.name}' AND id != {model.id}";
            var location = conn.QuerySingleOrDefault<Location>(sql);

            var result = new ReturnObject<long>();
            result.isCompleted = location != null ? true : false;
            return result;
        }

        public async Task<Location> GetLocationById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_location] WHERE id = {id}";
            var location = conn.QuerySingleOrDefault<Location>(sql);
            return location;
        }

        public async Task<List<Location>> GetAllLocations()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT * FROM [dbo].[tb_location]";
            var location = conn.Query<Location>(sql).ToList();
            return location;
        }

        //Addon Master Service 
        public async Task<List<ROPData>> GetAllMaterialWithROP(long vendor_id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT mp_rop.id, tb_material.name AS material_name, tb_material.raw_material_type_id, tb_material.is_active, tb_material.created_by, tb_material.created_at, tb_material.updated_by, tb_material.updated_at, tb_material_type.is_rop, tb_location.name AS location_name, mp_rop.material_id, mp_rop.location_id, tb_material.category_id, tb_material_vendor_list_id.vendor_id,tb_location.Status_sw
                FROM tb_material 
                INNER JOIN tb_material_type ON tb_material.raw_material_type_id = tb_material_type.id
                INNER JOIN mp_rop ON tb_material.id = mp_rop.material_id
                INNER JOIN tb_location ON mp_rop.location_id = tb_location.id
                INNER JOIN tb_material_vendor_list_id ON tb_material.id = tb_material_vendor_list_id.material_id
                WHERE vendor_id = {vendor_id}";
            var data = conn.Query<ROPData>(sql).ToList();
            return data;
        }


        public async Task<ReturnObject<long>> CreateCategory(Category model)
        {
            var result = new ReturnObject<long>();

            if (IsCategoryExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อหมวดหมู่ซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    INSERT INTO [dbo].[tb_category](
                        [name]
                        ,[fix_percentage_diff]               
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]    
                        ,[updated_at]
                    ) VALUES (
                        '{model.name}',                        
                        {model.fix_percentage_diff},
                        '{model.created_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}',
                        '{model.updated_by}',
                        '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    )";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("เพิ่มหมวดหมู่สำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }

            return result;
        }

        public async Task<ReturnObject<long>> EditCategory(Category model)
        {
            var result = new ReturnObject<long>();

            if (IsCategoryExists(model).Result.isCompleted)
            {
                result.isCompleted = false;
                result.message.Add("ชื่อหมวดหมู่ซ้ำในระบบ!");
            }
            else
            {
                try
                {
                    using var conn = await _db.CreateSqlConnectionAsync();
                    using var transaction = conn.BeginTransaction();

                    var sql = $@"
                    UPDATE [dbo].[tb_category]
                    SET
                        [name] = '{model.name}'
                        ,[fix_percentage_diff] = {model.fix_percentage_diff}      
                        ,[updated_at] = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", new CultureInfo("en-US"))}'
                    WHERE id = '{model.id}'";

                    await conn.ExecuteAsync(sql, model, transaction: transaction);
                    transaction.Commit();

                    result.isCompleted = true;
                    result.message.Add("แก้ไขข้อมูลหมวดหมู่สำเร็จ!");
                }
                catch (Exception ex)
                {
                    result.isCompleted = false;
                    result.message.Add(ex.Message);
                }
            }
            return result;
        }

        public async Task<ReturnObject<long>> DeleteCategory(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            using var transaction = conn.BeginTransaction();
            var sql = $"DELETE FROM [dbo].[tb_category] WHERE id = '{id}'";

            await conn.ExecuteAsync(sql, transaction: transaction);
            transaction.Commit();

            var result = new ReturnObject<long>();
            result.isCompleted = true;
            result.message.Add("ลบหมวดหมู่สำเร็จ!");

            return result;
        }

        public async Task<ReturnObject<long>> IsCategoryExists(Category model)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_category] WHERE name = '{model.name}' AND id != {model.id}";
            var location = conn.QuerySingleOrDefault<Category>(sql);

            var result = new ReturnObject<long>();
            result.isCompleted = location != null ? true : false;
            return result;
        }

        public async Task<Category> GetCategoryById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $"SELECT * FROM [dbo].[tb_category] WHERE id = {id}";
            var category = conn.QuerySingleOrDefault<Category>(sql);
            return category;
        }

        public async Task<List<Category>> GetAllCategories()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT * FROM [dbo].[tb_category]";
            var categories = conn.Query<Category>(sql).ToList();
            return categories;
        }

        public async Task<List<Category>> GetAllCategoriesIncludedRawMaterial()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT [tb_material].*, [tb_category].name AS category_name
                            FROM [dbo].[tb_material]
                            INNER JOIN [dbo].[tb_category] ON [tb_material].category_id = [tb_category].id";
            var data = conn.Query<Category>(sql).ToList();
            return data;
        }

        public async Task<List<ROPData>> GetAllCategoriesWithMaterialROP()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT DISTINCT tb_category.name AS category_name, tb_material.category_id, tb_material_type.is_rop, tb_material_type.name AS material_type_name
                FROM tb_category
                INNER JOIN tb_material ON tb_category.id = tb_material.category_id
                INNER JOIN tb_material_type ON tb_material_type.id = tb_material.raw_material_type_id
                WHERE tb_category.id IN (SELECT category_id FROM tb_material) AND tb_material_type.is_rop = 1";
            var categories = conn.Query<ROPData>(sql).ToList();
            return categories;
        }

        public async Task<List<ROPData>> GetAllCategoriesNonMaterialROP()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT DISTINCT tb_category.name AS category_name, tb_material.category_id, tb_material_type.is_rop, tb_material_type.name AS material_type_name
                FROM tb_category
                INNER JOIN tb_material ON tb_category.id = tb_material.category_id
                INNER JOIN tb_material_type ON tb_material_type.id = tb_material.raw_material_type_id
                WHERE tb_category.id IN (SELECT category_id FROM tb_material) AND tb_material_type.is_rop = 0";
            var categories = conn.Query<ROPData>(sql).ToList();
            return categories;
        }
        public async Task<List<Category>> GetCategoriesByCategory(long cate_id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @$"SELECT DISTINCT tb_vendor.id, tb_vendor.name, tb_material.category_id
FROM            tb_material_vendor_list_id INNER JOIN
                         tb_material ON tb_material_vendor_list_id.material_id = tb_material.id INNER JOIN
                         tb_vendor ON tb_material_vendor_list_id.vendor_id = tb_vendor.id
WHERE        (tb_material.is_active = 1) AND (tb_vendor.is_active = 1) AND (tb_material.category_id = {cate_id})";
            var data = conn.Query<Category>(sql).ToList();
            return data;
        }

    }
}
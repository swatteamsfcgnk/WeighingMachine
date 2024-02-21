using BNP.SCG.Web.Models;
using Dapper;
using Microsoft.CodeAnalysis;

namespace BNP.SCG.Web.Services
{
    public class FulfillService
    {
        IDatabaseSqlConnectionFactory _db;
        public FulfillService(IDatabaseSqlConnectionFactory _database)
        {
            _db = _database;
        }

        public async Task<List<FulfillData>> GetAll()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT 
                  CONVERT(nvarchar, date_in, 103) AS date_in_str
                  ,CONVERT(nvarchar, date_out, 103) AS date_out_str
	              ,id
	              ,date_in
	              ,date_out
                  ,[car_license]
                  ,[supplier_id]
                  ,[supplier_name]
                  ,[raw_material_id]
                  ,[raw_material_name]
                  ,[document_no]
                  ,[weight_in]
                  ,[weight_out]
                  ,[weight_diff]
                  ,[weight_in_2]
                  ,[weight_out_2]
                  ,[weight_diff_2]
                  ,[location_id]
                  ,[location_name]
                  ,[weight_register]
                  ,[percentage_diff]
                  ,[weight_silo_before]
                  ,[weight_silo_after]
                  ,[uniq_data]
                  ,[doc_status]
                  ,[weight_in_by]
                  ,[weight_in_at]
                  ,[weight_out_by]
                  ,[weight_out_at]
                  ,[location_id_2]
                  ,[location_name_2]
                  ,[percentage_diff_2]
                  ,[weight_silo_before_2]
                  ,[weight_silo_after_2]
              FROM tran_fulfill
              WHERE (is_deleted = 0)";
            var datas = conn.Query<FulfillData>(sql).ToList();
            return datas;
        }
        public async Task<FulfillData> GetById(long id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT CONVERT(nvarchar, date_in, 103) AS date_in_str, CONVERT(nvarchar, date_out, 103) AS date_out_str, id, date_in, date_out, car_license, supplier_id, supplier_name, raw_material_id, raw_material_name, document_no, weight_in, weight_out, weight_diff, weight_in_2, weight_out_2, weight_diff_2, location_id, location_name, location_id_2, location_name_2, weight_register,     percentage_diff, weight_silo_before, weight_silo_after, created_by, created_at, updated_by, updated_at, deleted_by, deleted_at, is_deleted, uniq_data, doc_status FROM [tran_fulfill]  WHERE (id=@id) AND   (is_deleted = 0)";
            var data = conn.Query<FulfillData>(sql, new { id }).FirstOrDefault();
            return data;
        }

        public async Task<decimal> GetWeight()
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = "SELECT TOP (1) weight FROM tbweight ORDER BY datetime DESC";
            var data = conn.Query<decimal>(sql).FirstOrDefault();
            return data;
        }

        public async Task<decimal> GetSiloWeight(string silo_location)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT 
                    tb_silo.value 
                FROM tb_location 
                INNER JOIN tb_silo ON tb_silo.location_id = tb_location.id
                    WHERE tb_location.name = '{silo_location}'";
            var data = conn.Query<decimal>(sql).FirstOrDefault();
            return data;
        }

        public async Task<ReturnObject<long>> InsertData(FulfillData model)
        {
            var result = new ReturnObject<long>();
            try
            {
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                //Check exists
                string query = @"SELECT * FROM tran_fulfill WHERE (car_license = @car_license) AND (document_no = @document_no) AND (doc_status = N'NEW') AND   (is_deleted = 0)";
                var check = conn.Query<FulfillData>(query, model, transaction: transaction).FirstOrDefault();
                if (check != null)
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    return result;
                }

                query = @"
                    INSERT INTO [dbo].[tran_fulfill]
                        ([date_in]
                        ,[date_out]
                        ,[car_license]
                        ,[supplier_id]
                        ,[supplier_name]
                        ,[raw_material_id]
                        ,[raw_material_name]
                        ,[document_no]
                        ,[weight_in]
                        ,[weight_out]
                        ,[weight_diff]
                        ,[weight_in_2]
                        ,[weight_out_2]
                        ,[weight_diff_2]
                        ,[location_id]
                        ,[location_name]
                        ,[location_id_2]
                        ,[location_name_2]
                        ,[weight_register]
                        ,[percentage_diff]
                        ,[weight_silo_before]
                        ,[weight_silo_after]
                        ,[created_by]
                        ,[created_at]
                        ,[updated_by]
                        ,[updated_at]
                        ,[uniq_data]
                        ,[doc_status]
                        ,[is_deleted]
                        )
                    VALUES
                        (@date_in
                        ,@date_out
                        ,@car_license
                        ,@supplier_id
                        ,@supplier_name
                        ,@raw_material_id
                        ,@raw_material_name
                        ,@document_no
                        ,@weight_in
                        ,@weight_out
                        ,@weight_diff
                        ,@weight_in_2
                        ,@weight_out_2
                        ,@weight_diff_2
                        ,@location_id
                        ,@location_name
                        ,@location_id_2
                        ,@location_name_2
                        ,@weight_register
                        ,@percentage_diff
                        ,@weight_silo_before
                        ,@weight_silo_after
                        ,@created_by
                        ,@created_at
                        ,@updated_by
                        ,@updated_at
                        ,@uniq_data
                        ,'NEW'
                        ,0
                        )";

                await conn.ExecuteAsync(query, model, transaction: transaction);

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

        public async Task<ReturnObject<long>> UpdateData(FulfillData model)
        {
            var result = new ReturnObject<long>();
            try
            {
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                //Check exists
                string query = @"SELECT * FROM   tran_fulfill WHERE (id = @id) AND  (is_deleted = 0)";
                var check = conn.Query<FulfillData>(query, model, transaction: transaction).FirstOrDefault();
                if (check == null)
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ไม่พบข้อมูล !!");
                    return result;
                }

                //Check Status
                if (check.doc_status.StartsWith("WEIGHT"))
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ข้อมูลการลงทะเบียนมีการชั่งน้ำหนักแล้ว ไม่สามารถแก้ไขได้ !!");
                    return result;
                }

                query = @"UPDATE [dbo].[tran_fulfill]
                   SET 
                      [car_license] = @car_license
                      ,[supplier_id] = @supplier_id
                      ,[supplier_name] = @supplier_name
                      ,[raw_material_id] = @raw_material_id
                      ,[raw_material_name] = @raw_material_name
                      ,[document_no] = @document_no
                      ,[weight_register] = @weight_register
                      ,[location_id] = @location_id
                      ,[location_name] = @location_name
                      ,[updated_by] = @updated_by
                      ,[updated_at] = @updated_at
                WHERE (id=@id)";

                await conn.ExecuteAsync(query, model, transaction: transaction);
                transaction.Commit();
                result.data = model.id;
                result.isCompleted = true;
                result.message.Add("บันทึกสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> UpdateWeightIN(FulfillData model)
        {
            var result = new ReturnObject<long>();
            try
            {
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                //Check exists
                string query = @"SELECT * FROM tran_fulfill WHERE (id = @id) AND (is_deleted = 0)";
                var check = conn.Query<FulfillData>(query, model, transaction: transaction).FirstOrDefault();
                if (check == null)
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ไม่พบข้อมูลการลงทะเบียน !!");
                    return result;
                }

                if (!check.doc_status.Equals("NEW"))
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ข้อมูลมีการบันทึกการชั่งหนักเรียบร้อยแล้ว ไม่สามารถบันทึกซ้ำได้ !!");
                    return result;
                }

                query = @"UPDATE [dbo].[tran_fulfill]
                    SET 
                        weight_in = @weight_in
                        ,weight_diff = @weight_diff
                        ,weight_in_by = @weight_in_by
                        ,weight_in_at = @weight_in_at 
                        ,doc_status = 'WEIGHT-IN' 
                        ,weight_silo_before = @weight_silo_before
                    WHERE (id=@id)";

                await conn.ExecuteAsync(query, model, transaction: transaction);
                transaction.Commit();
                result.data = model.id;
                result.isCompleted = true;
                result.message.Add("บันทึกสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> UpdateWeightIN2(FulfillData model)
        {
            var result = new ReturnObject<long>();
            try
            {
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                //Check exists
                string query = @"SELECT * FROM tran_fulfill WHERE (id = @id) AND   (is_deleted = 0)";
                var check = conn.Query<FulfillData>(query, model, transaction: transaction).FirstOrDefault();
                if (check == null)
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ไม่พบข้อมูลการลงทะเบียน !!");
                    return result;
                }

                if (!check.doc_status.Equals("WEIGHT-OUT"))
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ข้อมูลมีการบันทึกการชั่งหนักเรียบร้อยแล้ว ไม่สามารถบันทึกซ้ำได้ !!");
                    return result;
                }

                query = @"UPDATE [dbo].[tran_fulfill]
                    SET 
                        [weight_in_2] = @weight_in_2
                        ,[weight_diff_2] = @weight_in_2
                        ,[weight_in_by] = @weight_in_by
                        ,[weight_in_at] = @weight_in_at
                        ,[doc_status] = 'WEIGHT-IN-2'
                        ,weight_silo_before_2 = @weight_silo_before_2
                    WHERE (id=@id)";

                await conn.ExecuteAsync(query, model, transaction: transaction);
                transaction.Commit();
                result.data = model.id;
                result.isCompleted = true;
                result.message.Add("บันทึกสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> UpdateWeightOUT(FulfillData model)
        {
            var result = new ReturnObject<long>();
            try
            {
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                //Check exists
                string query = @"SELECT * FROM   tran_fulfill WHERE (id = @id) AND   (is_deleted = 0)";
                var check = conn.Query<FulfillData>(query, model, transaction: transaction).FirstOrDefault();
                if (check == null)
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ไม่พบข้อมูลการลงทะเบียน !!");
                    return result;
                }

                if (!check.doc_status.Equals("WEIGHT-IN"))
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ข้อมูลมีการบันทึกการชั่งเบาเรียบร้อยแล้ว ไม่สามารถบันทึกซ้ำได้ !!");
                    return result;
                }

                query = @"UPDATE [dbo].[tran_fulfill]
                    SET 
                        weight_out = @weight_out
                        ,weight_diff = @weight_diff                        
                        ,percentage_diff = @percentage_diff
                        ,weight_out_by = @weight_out_by
                        ,date_out = @date_out
                        ,weight_out_at = @weight_out_at
                        ,doc_status = 'WEIGHT-OUT'
                        ,weight_silo_after = @weight_silo_after
                    WHERE (id=@id)";

                await conn.ExecuteAsync(query, model, transaction: transaction);
                transaction.Commit();
                result.data = model.id;
                result.isCompleted = true;
                result.message.Add("บันทึกสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<ReturnObject<long>> UpdateWeightOUT2(FulfillData model)
        {
            var result = new ReturnObject<long>();
            try
            {
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                //Check exists
                string query = @"SELECT * FROM tran_fulfill WHERE (id = @id) AND (is_deleted = 0)";
                var check = conn.Query<FulfillData>(query, model, transaction: transaction).FirstOrDefault();
                if (check == null)
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ไม่พบข้อมูลการลงทะเบียน !!");
                    return result;
                }

                if (!check.doc_status.Equals("WEIGHT-IN-2"))
                {
                    transaction.Commit();
                    result.data = -1;
                    result.isCompleted = false;
                    result.message.Add("ข้อมูลมีการบันทึกการชั่งเบาเรียบร้อยแล้ว ไม่สามารถบันทึกซ้ำได้ !!");
                    return result;
                }

                query = @"UPDATE [dbo].[tran_fulfill]
                    SET 
                        weight_out_2 = @weight_out_2
                        ,weight_diff_2 = @weight_diff_2
                        ,percentage_diff_2 = @percentage_diff_2
                        ,weight_out_by = @weight_out_by  
                        ,date_out = @date_out
                        ,weight_out_at = @weight_out_at
                        ,doc_status = 'WEIGHT-OUT-2' 
                        ,weight_silo_after_2 = @weight_silo_after_2
                    WHERE (id=@id)";

                await conn.ExecuteAsync(query, model, transaction: transaction);
                transaction.Commit();
                result.data = model.id;
                result.isCompleted = true;
                result.message.Add("บันทึกสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<FulfillData> GetSecondLocation(long material_id, long location_id)
        {
            var data = new FulfillData();

            //open transaction
            using var conn = await _db.CreateSqlConnectionAsync();

            //Get rop_id
            string query = $@"SELECT * FROM mp_rop WHERE material_id = {material_id} AND location_id = {location_id}";
            var _rop = conn.Query<ROPData>(query).FirstOrDefault();
            if (_rop == null)
            {
                return data;
            }

            query = $@"SELECT mp_rop_addon.rop_id, mp_rop_addon.location_id AS location_id_2, tb_location.name AS location_name_2, mp_rop.material_id
                FROM mp_rop_addon
                INNER JOIN mp_rop ON mp_rop_addon.rop_id = mp_rop.id
                INNER JOIN tb_location ON mp_rop_addon.location_id = tb_location.id
                WHERE mp_rop.material_id = {_rop.material_id}";
            data = conn.Query<FulfillData>(query).FirstOrDefault();
            return data;
        }

        public async Task<FulfillData> GetDataByQR(string qr)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = @"SELECT CONVERT(nvarchar, tran_fulfill.date_in, 103) AS date_in_str, 
                CONVERT(nvarchar, tran_fulfill.date_out, 103) AS date_out_str, 
                tran_fulfill.id, 
                tran_fulfill.date_in, 
                tran_fulfill.date_out, 
                tran_fulfill.car_license, 
                tran_fulfill.supplier_id, 
                tran_fulfill.supplier_name, 
                tran_fulfill.raw_material_id, 
                tran_fulfill.raw_material_name, 
                tran_fulfill.document_no, 
                tran_fulfill.weight_in, 
                tran_fulfill.weight_out,
                tran_fulfill.weight_diff,
                tran_fulfill.weight_in_2,
                tran_fulfill.weight_out_2,
                tran_fulfill.weight_diff_2,
                tran_fulfill.location_id,
                tran_fulfill.location_name,
                tran_fulfill.location_id_2, 
                tran_fulfill.location_name_2, 
                tran_fulfill.weight_register, 
                tran_fulfill.percentage_diff, 
                tran_fulfill.weight_silo_before, 
                tran_fulfill.weight_silo_after, 
                tran_fulfill.created_by, 
                tran_fulfill.created_at, 
                tran_fulfill.updated_by, 
                tran_fulfill.updated_at, 
                tran_fulfill.deleted_by, 
                tran_fulfill.deleted_at, 
                tran_fulfill.is_deleted,
                tran_fulfill.uniq_data,
                tran_fulfill.doc_status,
                tb_material.category_id,
                tb_category.fix_percentage_diff as category_fix_percentage_diff
                FROM [tran_fulfill] 
                INNER JOIN tb_material ON tran_fulfill.raw_material_id = tb_material.id 
                INNER JOIN tb_category ON tb_material.category_id = tb_category.id 
                WHERE (uniq_data LIKE @qr) AND (is_deleted = 0)";
            var data = conn.Query<FulfillData>(sql, new { qr }).FirstOrDefault();
            return data;
        }

        public async Task<FulfillData> GetDataByKeyword(string keyword)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            try
            {
                string sql = "SELECT * FROM [tran_fulfill] WHERE  ((uniq_data LIKE @keyword) OR (car_license=@keyword) OR (document_no = @keyword)) AND (doc_status <> 'COMPLETED') AND (is_deleted = 0)";
                var data = conn.Query<FulfillData>(sql, new { keyword }).LastOrDefault();
                return data;
            }
            catch
            {
                string sql = "SELECT * FROM [tran_fulfill] WHERE  ((car_license=@keyword) OR (document_no = @keyword)) AND (doc_status <> 'COMPLETED') AND   (is_deleted = 0)";
                var data = conn.Query<FulfillData>(sql, new { keyword }).LastOrDefault();
                return data;
            }
        }

        public async Task<List<RMRemainData>> CalculateSilo(long cat_id, long vendor_id)
        {
            using var conn = await _db.CreateSqlConnectionAsync();
            string sql = $@"SELECT mp_rop.id,
                tb_material.name AS material_name,
                tb_location.name AS location_name,
                tb_silo.value AS remain_qty,
                mp_rop.rop,
                mp_rop.location_id,
                mp_rop.material_id,
                tb_silo.value / mp_rop.usge_qty AS cal,
                tb_material.category_id
            FROM mp_rop
            INNER JOIN tb_location ON mp_rop.location_id = tb_location.id
            INNER JOIN tb_material ON mp_rop.material_id = tb_material.id
            INNER JOIN tb_silo ON tb_location.id = tb_silo.location_id AND tb_material.id = tb_silo.material_id
            INNER JOIN tb_material_vendor_list_id ON mp_rop.material_id = tb_material_vendor_list_id.material_id
                WHERE (tb_material.is_active = 1)
                AND (tb_material.category_id = {cat_id})
                AND (tb_material_vendor_list_id.vendor_id = {vendor_id})
                AND (tb_silo.value IS NOT NULL)
                AND tb_silo.value < mp_rop.rop";

            var data = conn.Query<RMRemainData>(sql).ToList();
            return data;
        }

        public async Task<List<FulfillData>> GetAllByStatus(string[] doc_status)
        {
            using var conn = await _db.CreateSqlConnectionAsync();

            string _where = "(";
            foreach (var item in doc_status)
            {
                _where += " doc_status LIKE '" + item + "%' OR";
            }
            _where = _where.Substring(0, _where.Length - 2) + ")";
            string sql = @"SELECT CONVERT(nvarchar, date_in, 103) AS date_in_str, CONVERT(nvarchar, date_out, 103) AS date_out_str, id, date_in, date_out, car_license, supplier_id, supplier_name, raw_material_id, raw_material_name, document_no, weight_in, weight_out, weight_diff, weight_in_2, weight_out_2, weight_diff_2, location_id, location_name, weight_register, percentage_diff, weight_silo_before, weight_silo_after, created_by, created_at, updated_by, updated_at, deleted_by, deleted_at, is_deleted, uniq_data, doc_status FROM [tran_fulfill] WHERE (1=1) AND ( " + _where + ") AND   (is_deleted = 0) ";
            var datas = conn.Query<FulfillData>(sql).ToList();
            return datas;
        }

        public async Task<ReturnObject<long>> Printing(string qr)
        {
            var result = new ReturnObject<long>();
            try
            {
                string[] _data = qr.Split('|');
                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();

                string sql = @"INSERT INTO [dbo].[tb_print]
                   (print_data_qr
                   ,car_license        
                   ,supplier_name
                   ,location_name
                   ,location_name_2
               ,is_print)
                VALUES
                   (@print_data_qr
                   ,@car_license               
                   ,@supplier_name
                   ,@location_name
                   ,@location_name_2
                   ,0)";

                await conn.ExecuteAsync(sql, new { print_data_qr = _data[0], car_license = _data[1], supplier_name = _data[2], location_name = _data[3], location_name_2 = _data[4] });

                result.isCompleted = true;
                result.message.Add("บันทึกสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }
            return result;
        }

        public async Task<ReturnObject<long>> Delete(int id, string user_name)
        {
            var result = new ReturnObject<long>();
            try
            {
                FulfillData model = new FulfillData()
                {
                    deleted_at = DateTime.Now,
                    deleted_by = user_name,
                    is_deleted = true,
                    id = id
                };

                //open transaction
                using var conn = await _db.CreateSqlConnectionAsync();
                using var transaction = conn.BeginTransaction();

                string query = $@"UPDATE [dbo].[tran_fulfill]
                    SET 
                        deleted_at = @deleted_at, 
                        deleted_by = @deleted_by,
                        is_deleted = @is_deleted 
                    WHERE (id=@id)";

                await conn.ExecuteAsync(query, model, transaction: transaction);
                transaction.Commit();
                result.data = id;
                result.isCompleted = true;
                result.message.Add("แก้ไขสำเร็จ!");

            }
            catch (Exception ex)
            {
                result.isCompleted = false;
                result.message.Add(ex.Message);
            }

            return result;
        }

        public async Task<bool> PrintFinish(PrintFinishModel model)
        {
            var res = false;
            try
            {
                using var conn = await _db.CreateSqlConnectionAsync();
                string sql = $@"insert into tb_print_finish 
                 (car_license,is_print,create_date,diff,document_no,supplier_name,raw_material_name,location_name,weight_register,weight_in,weight_out,percentage_diff,weight_in_at,weight_out_at,Ticket_Diff) 
                values ('{model.car_license}',0,getdate(),'{model.diff}','{model.document_no}','{model.supplier_name}','{model.raw_material_name}','{model.location_name}','{model.weight_register}','{model.weight_in}','{model.weight_out}','{model.percentage_diff}','{model.weight_in_at}','{model.weight_out_at}','{model.Ticket_Diff}')";

                //string sql = $@"insert into tb_print_finish 
                //(car_license,is_print,create_date,diff) 
                //values ('{model.car_license}',0,getdate(),'{model.diff}')";
                await conn.ExecuteAsync(sql);
                res = true;
            }
            catch (Exception ex)
            {
                res = false;
            }
            return res;
        }

    }
}

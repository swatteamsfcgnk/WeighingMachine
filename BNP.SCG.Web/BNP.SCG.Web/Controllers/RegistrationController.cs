using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly FulfillService _serviceFulfill;
        private readonly ROPService _serviceROP;
        private readonly MasterDataService _serviceMaster;
        private readonly InfoService _serviceInfo;
        private readonly IWebHostEnvironment _env;

        public RegistrationController(FulfillService serviceFulfill, IWebHostEnvironment env, MasterDataService serviceMaster, ROPService serviceROP, InfoService serviceInfo)
        {
            _serviceFulfill = serviceFulfill;
            _env = env;
            _serviceMaster = serviceMaster;
            _serviceROP = serviceROP;
            _serviceInfo = serviceInfo;
        }

        public IActionResult Index(long RM, long Vendor)
        {
            //Supplier
            ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;

            //Get ROP Mapping First
            var _rmrop = _serviceROP.GetROPById(RM).Result;
            var _rm = _serviceMaster.GetRawMaterialById(_rmrop.material_id).Result;
            if (_rmrop == null)
            {
                TempData["ErrorMessage"] = "วัตถุดิบต้องมีการตั้งค่าลงวัตถุดิบและ ROP เสียก่อน !";
                return RedirectToAction("Calculate");
            }
            else
            {
                var model = new FulfillData()
                {
                    raw_material_id = _rm.id,
                    raw_material_name = _rm.name,
                    location_id = _rmrop.location_id,
                    location_name = _rmrop.location_name,
                    supplier_id = Vendor
                };
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Index(FulfillData model)
        {
            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            model.date_in = DateTime.Now;
            model.weight_out = 0; model.weight_in = 0;
            model.weight_diff = 0;
            model.created_at = DateTime.Now; model.created_by = first_name;
            model.updated_at = DateTime.Now; model.updated_by = first_name;
            model.uniq_data = Guid.NewGuid();

            var second_location = _serviceFulfill.GetSecondLocation(model.raw_material_id, model.location_id).Result;
            if (second_location != null)
            {
                model.location_id_2 = second_location.location_id_2;
                model.location_name_2 = second_location.location_name_2;
            }

            var result = _serviceFulfill.InsertData(model).Result;
            if (!result.isCompleted)
            {
                //Supplier
                ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
                model.error_message = "มีการลงทะเบียนเรียบร้อยแล้ว และยังไม่เสร็จสมบูรณ์  !!";
                return View(model);
            }
            else
            {
                //Force to Print QR Code
                PrintQR(result.data);
                TempData["PopupRegister"] = _serviceInfo.GetRegisterMessage().Result;
                TempData["Message"] = result.message[0];
                //return RedirectToAction("Form", "Registration", new { id = result.data });
                return RedirectToAction("Category", "Registration");
            }
        }

        public IActionResult Category()
        {
            //Get All Category
            var _rop = _serviceMaster.GetAllCategoriesWithMaterialROP().Result;
            ViewBag.categoriesROP = _rop;
            ViewBag.categoriesROPDistinct = _rop.DistinctBy(d => d.material_type_name).ToList();

            var _nonRop = _serviceMaster.GetAllCategoriesNonMaterialROP().Result;
            ViewBag.categoriesNonROP = _nonRop;
            ViewBag.categoriesNonROPDistinct = _nonRop.DistinctBy(d => d.material_type_name).ToList();
            return View();
        }

        public IActionResult Calculate(long Cate, long Vendor)
        {
            //Get All RawMaterial
            var _RawMaterial = _serviceMaster.GetAllMaterialWithROP(Vendor).Result.Where(w => w.is_active && w.category_id == Cate).ToList();

            //Go Register if non rop and on 1 material.
            var _NonRop = _RawMaterial.Where(w => w.is_rop == false).ToList();
            if (_NonRop.Count == 1)
            {
                return RedirectToAction("Index", "Registration", new { RM = _NonRop[0].id, Vendor = Vendor });
            }

            return View(_RawMaterial);
        }

        public IActionResult Form(long? id)
        {
            //Supplier
            ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
            if (!id.HasValue)
            {
                ViewBag.qrCodeImage = "NO";
                return View();
            }
            else
            {
                var _regis = _serviceFulfill.GetById(id.Value).Result;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(_regis.uniq_data.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                var qrCodeAsBitmap = qrCode.GetGraphic(20);

                string base64String = Convert.ToBase64String(BitmapToByteArray(qrCodeAsBitmap));
                string htmlPictureTag = base64String;

                ViewBag.qrCodeImage = htmlPictureTag;

                return View(_regis);
            }
        }

        [HttpPost]
        public IActionResult Form(FulfillData model)
        {
            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            model.weight_diff = 0; model.weight_in = 0; model.weight_out = 0;
            model.updated_at = DateTime.Now; model.updated_by = first_name;
            var result = _serviceFulfill.UpdateData(model).Result;
            if (!result.isCompleted)
            {
                //Supplier
                ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
                model.error_message = result.message[0];
                var _regis = _serviceFulfill.GetById(model.id).Result;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(_regis.uniq_data.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                var qrCodeAsBitmap = qrCode.GetGraphic(20);

                string base64String = Convert.ToBase64String(BitmapToByteArray(qrCodeAsBitmap));
                string htmlPictureTag = base64String;

                ViewBag.qrCodeImage = htmlPictureTag;
                return View(model);
            }
            else
            {
                TempData["Message"] = "แก้ไขข้อมูลการลงทะเบียนเรียบร้อย";
                return RedirectToAction("Form", "Registration", new { id = result.data });
            }
        }

        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        [HttpPost]
        public virtual ActionResult getDataByKeyword(string keyword)
        {
            try
            {
                var result = _serviceFulfill.GetDataByKeyword(keyword).Result;
                if (result != null)
                {
                    return Json(new { status = "success", model = result });
                }
                else
                {
                    return Json(new { status = "error", message = "ไม่พบข้อมูลที่ต้องการค้นหา !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }

        }

        [HttpPost]
        public virtual ActionResult CalSilo(long cat_id, long vendor_id)
        {
            try
            {
                var _tmp = _serviceFulfill.CalculateSilo(cat_id, vendor_id).Result.OrderBy(o => o.material_name).ThenBy(t => t.cal).ToList();
                //var _tmp = _serviceFulfill.CalculateSilo(cat_id, vendor_id).Result.OrderBy(o => o.material_name).ThenByDescending(t => t.cal).ToList();
                var result = new List<RMRemainData>();
                var fulfillReg = _serviceFulfill.GetAllByStatus(new string[] { "NEW", "WEIGHT-IN" }).Result;
                foreach (var item in _tmp)
                {
                    var _data = fulfillReg.Where(w => w.location_id == item.location_id && w.raw_material_id == item.material_id).FirstOrDefault();
                    if (_data == null)
                    {
                        result.Add(item);
                    }
                }
                var calReturn = new List<RMRemainData>();
                string _sameRM = string.Empty;
                foreach (var item in result)
                {
                    if (string.IsNullOrWhiteSpace(_sameRM))
                    {
                        _sameRM = item.material_name;
                        calReturn.Add(item);
                    }
                    else
                    {
                        if (item.material_name == _sameRM)
                        {
                            continue;
                        }
                        else
                        {
                            _sameRM = item.material_name;
                            calReturn.Add(item);
                        }
                    }
                }
                if (calReturn != null)
                {
                    return Json(new { status = "success", model = calReturn });
                }
                else
                {
                    return Json(new { status = "error", message = "ไม่พบข้อมูลที่ต้องการค้นหา !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }

        }

        //Print
        //[HttpGet]
        //public async Task<IActionResult> PrintQR(long id)
        //{
        //    try
        //    {
        //        var result = _serviceFulfill.GetById(id).Result;

        //        string _qr = String.Format("{0}|{1}", result.uniq_data.ToString(), result.location_name);
        //        byte[] _t = Encoding.ASCII.GetBytes(_qr);
        //        var stream = new MemoryStream(_t);
        //        return File(stream, "text/plain", "SCGDATA.txt");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //        return View();
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> PrintQR(long id)
        {
            try
            {
                var result = _serviceFulfill.GetById(id).Result;
                string _qr = String.Format("{0}|{1}|{2}|{3}|{4}", result.uniq_data.ToString(), result.car_license, result.supplier_name, result.location_name, result.location_name_2);
                var _pr = _serviceFulfill.Printing(_qr).Result;
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public virtual ActionResult GetSupplierData(long category_id)
        {
            try
            {
                return Json(new { status = "success", message = _serviceMaster.GetCategoriesByCategory(category_id).Result });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public virtual ActionResult Delete(int id)
        {
            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            var result = _serviceFulfill.Delete(id, first_name).Result;

            if (result.isCompleted)
            {
                return Json(new { status = "success", result = result });
            }
            else
            {
                return Json(new { status = "error", result = result });
            }

        }
    }
}

using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerService _serviceCustomer;
        private readonly GateService _serviceGate;


        public CustomerController(CustomerService serviceCustomer, GateService serviceGate)
        {
            _serviceCustomer = serviceCustomer;
            _serviceGate = serviceGate;
        }

        public IActionResult Index()
        {
            var model = new Customer();
            return View(model);
        }


        [HttpPost]
        public IActionResult Index(Customer model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                var result = _serviceCustomer.EditCustomer(model).Result;
                if (!result.isCompleted)
                {
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
                TempData["NotiMessageUpdatedUser"] = result.message[1];
            }
            else
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.created_by = first_name;
                model.updated_by = first_name;
                model.uniq_data = Guid.NewGuid();

                var result = _serviceCustomer.CreateCustomer(model).Result;
                if (!result.isCompleted)
                {
                    //ViewBag.users = _serviceCustomer.GetAllCustomers().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                PrintQR(result.data);
                TempData["Message"] = result.message[0];

                //QRCodeGenerator qrGenerator = new QRCodeGenerator();
                //QRCodeData qrCodeData = qrGenerator.CreateQrCode(_regis.uniq_data.ToString(), QRCodeGenerator.ECCLevel.Q);
                //QRCode qrCode = new QRCode(qrCodeData);
                //var qrCodeAsBitmap = qrCode.GetGraphic(20);

                //string base64String = Convert.ToBase64String(BitmapToByteArray(qrCodeAsBitmap));
                //string htmlPictureTag = base64String;
            }

            return RedirectToAction("Category", "Registration");
        }

        public IActionResult Form(long? id)
        {
            if (!id.HasValue)
            {
                ViewBag.qrCodeImage = "NO";
                return View();
            }
            else
            {
                var _customer = _serviceCustomer.GetCustomerById(Convert.ToInt32(id)).Result;
                if (_customer != null)
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(_customer.uniq_data.ToString(), QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);
                    var qrCodeAsBitmap = qrCode.GetGraphic(20);

                    string base64String = Convert.ToBase64String(BitmapToByteArray(qrCodeAsBitmap));
                    string htmlPictureTag = base64String;

                    ViewBag.qrCodeImage = htmlPictureTag;

                    return View(_customer);
                }

                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Form(Customer model)
        {
            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            model.updated_at = DateTime.Now; model.updated_by = first_name;
            var result = _serviceCustomer.EditCustomer(model).Result;
            if (!result.isCompleted)
            {
                model.error_message = result.message[0];
                var _regis = _serviceCustomer.GetCustomerById(Convert.ToInt32(model.id)).Result;

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
                return RedirectToAction("Form", "Customer", new { id = model.id });
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
                var result = _serviceCustomer.GetDataByKeyword(keyword).Result;
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

        public IActionResult Gate3()
        {
            return View();
        }

        [HttpPost]
        public virtual ActionResult getDataByQR(string qr)
        {
            try
            {
                var result = _serviceCustomer.GetDataByQR(qr).Result;
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

        [HttpGet]
        public async Task<IActionResult> PrintQR(long id)
        {
            try
            {
                var result = _serviceCustomer.GetCustomerById(Convert.ToInt32(id)).Result;
                string _qr = String.Format("{0}|{1}|{2}", result.uniq_data.ToString(), result.car_license, result.name);
                var _pr = _serviceCustomer.Printing(_qr).Result;
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        public void OpenGate(string gate_name, string process, int? fulfill_id, int? customer_id)
        {
            var _gate = new Gate()
            {
                gate_name = gate_name,
                process = process,
                fulfill_id = fulfill_id,
                customer_id = customer_id
            };

            var _x = _serviceGate.OpenGate(_gate).Result;
            Thread.Sleep(5000);
            _x = _serviceGate.CloseGate(_gate).Result;
        }

    }
}

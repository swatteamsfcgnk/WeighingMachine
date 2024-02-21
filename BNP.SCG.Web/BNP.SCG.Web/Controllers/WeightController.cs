using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    [Authorize]
    public class WeightController : Controller
    {
        private readonly FulfillService _serviceFulfill;
        private readonly ROPService _serviceROP;
        private readonly MasterDataService _serviceMaster;
        private readonly InfoService _serviceInfo;
        private readonly GateService _serviceGate;
        private readonly CustomerService _serviceCustomer;
        private readonly IWebHostEnvironment _env;
        public WeightController(FulfillService serviceFulfill, IWebHostEnvironment env, MasterDataService serviceMaster, ROPService serviceROP, InfoService serviceInfo, GateService serviceGate, CustomerService serviceCustomer)
        {
            _serviceFulfill = serviceFulfill;
            _env = env;
            _serviceMaster = serviceMaster;
            _serviceROP = serviceROP;
            _serviceInfo = serviceInfo;
            _serviceGate = serviceGate;
            _serviceCustomer = serviceCustomer;
        }

        public IActionResult Hard(long? id)
        {
            if (id.HasValue)
            {
                return View(_serviceFulfill.GetById(id.Value).Result);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult Hard(FulfillData model)
        {
            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            model.weight_diff = model.weight_in;
            model.weight_in_at = DateTime.Now; model.weight_in_by = first_name;

            if (!model.second_load)
            {
                model.weight_silo_before = _serviceFulfill.GetSiloWeight(model.location_name).Result;
                var result = _serviceFulfill.UpdateWeightIN(model).Result;
                if (!result.isCompleted)
                {
                    //Supplier
                    ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
                    model.error_message = result.message[0];
                    return View(model);
                }
                else
                {
                    TempData["Message"] = "บันทึกข้อมูลการชั่งหนักเรียบร้อย";
                    TempData["PopupWeightIn"] = _serviceInfo.GetWeightInMessage().Result;
                    TempData["FulfillIdWeihtIn"] = Convert.ToInt32(model.id);
                    return RedirectToAction("Hard", "Weight", new { id = result.data });
                }
            }
            else
            {
                model.weight_silo_before_2 = _serviceFulfill.GetSiloWeight(model.location_name).Result;
                var result = _serviceFulfill.UpdateWeightIN2(model).Result;
                if (!result.isCompleted)
                {
                    //Supplier
                    ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
                    model.error_message = result.message[0];
                    return View(model);
                }
                else
                {
                    TempData["Message"] = "บันทึกข้อมูลการชั่งหนักเรียบร้อย";
                    TempData["PopupWeightIn"] = _serviceInfo.GetWeightInMessage().Result;
                    TempData["FulfillIdWeihtIn"] = Convert.ToInt32(model.id);
                    return RedirectToAction("Hard", "Weight", new { id = result.data });
                }
            }


        }

        public IActionResult Low(long? id)
        {
            if (id.HasValue)
            {
                return View(_serviceFulfill.GetById(id.Value).Result);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public IActionResult Low(FulfillData model)
        {
            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            model.weight_diff = model.weight_in - model.weight_out; 
            model.weight_out_at = DateTime.Now; model.weight_out_by = first_name;
            model.date_out = DateTime.Now;
       

            if (!model.second_load)
            {
                model.weight_silo_after = _serviceFulfill.GetSiloWeight(model.location_name).Result;
                var result = _serviceFulfill.UpdateWeightOUT(model).Result;
                if (!result.isCompleted)
                {
                    //Supplier
                    ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
                    model.error_message = result.message[0];
                    return View(model);
                }
                else
                {
                    // Print Finish
                    var newmdel = _serviceFulfill.GetById(model.id).Result;
                    var printFinish = new PrintFinishModel()
                    {
                        car_license = newmdel.car_license,
                        diff = newmdel.weight_diff,
                        document_no = newmdel.document_no,
                        supplier_name = newmdel.supplier_name,
                        raw_material_name = newmdel.raw_material_name,
                        location_name = newmdel.location_name,
                        weight_register = newmdel.weight_register,
                        weight_in = newmdel.weight_in,
                        weight_out = newmdel.weight_out,
                        percentage_diff = newmdel.percentage_diff,
                        weight_in_at = newmdel.date_in,
                        weight_out_at = newmdel.date_out,
                        Ticket_Diff = newmdel.weight_register - newmdel.weight_diff,

                    };
                    _serviceFulfill.PrintFinish(printFinish);
                    TempData["Message"] = "บันทึกข้อมูลการชั่งเบาเรียบร้อย";
                    TempData["PopupWeightOut"] = _serviceInfo.GetWeightOutMessage().Result;
                    TempData["FulfillIdWeihtOut"] = Convert.ToInt32(model.id);
                    return RedirectToAction("Low", "Weight", new { id = result.data });
                }
            }
            else
            {
                model.weight_silo_after_2 = _serviceFulfill.GetSiloWeight(model.location_name).Result;
                var result = _serviceFulfill.UpdateWeightOUT2(model).Result;
                if (!result.isCompleted)
                {
                    //Supplier
                    ViewBag.SupplierData = _serviceMaster.GetAllVendors().Result;
                    model.error_message = result.message[0];
                    return View(model);
                }
                else
                {
                    // Print Finish
                    var newmdel = _serviceFulfill.GetById(model.id).Result;
                    var printFinish = new PrintFinishModel()
                    {
                        car_license = newmdel.car_license,
                        diff = newmdel.weight_diff,
                        document_no = newmdel.document_no,
                        supplier_name = newmdel.supplier_name,
                        raw_material_name = newmdel.raw_material_name,
                        location_name = newmdel.location_name,
                        weight_register = newmdel.weight_register,
                        weight_in = newmdel.weight_in,
                        weight_out = newmdel.weight_out,
                        percentage_diff = newmdel.percentage_diff,
                        weight_in_at = newmdel.date_in,
                        weight_out_at = newmdel.date_out,
                        Ticket_Diff = newmdel.weight_register - newmdel.weight_diff,

                    };
                    _serviceFulfill.PrintFinish(printFinish);
                    TempData["Message"] = "บันทึกข้อมูลการชั่งเบาเรียบร้อย";
                    TempData["PopupWeightOut"] = _serviceInfo.GetWeightOutMessage().Result;
                    TempData["FulfillIdWeihtOut"] = Convert.ToInt32(model.id);
                    return RedirectToAction("Low", "Weight", new { id = result.data });
                }
            }
        }

        [HttpPost]
        public virtual ActionResult getDataByQR(string qr, string process)
        {
            try
            {
                var fulfill = _serviceFulfill.GetDataByQR(qr).Result;
                if (fulfill != null)
                {
                    if (process == "weight_in" && fulfill.doc_status == "WEIGHT-OUT" && fulfill.location_id_2 != 0 || process == "weight_out" && fulfill.doc_status == "WEIGHT-IN-2" && fulfill.location_id_2 != 0)
                    {
                        fulfill.second_load = true;
                    }
                    return Json(new { status = "success", model = fulfill, type = "fulfill" });
                }
                else
                {
                    var customer = _serviceCustomer.GetDataByQR(qr).Result;
                    if (customer != null)
                    {
                        return Json(new { status = "success", model = customer, type = "customer" });
                    }
                    else
                    {
                        return Json(new { status = "error", message = "ไม่พบข้อมูลที่ต้องการค้นหา !" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }

        }

        [HttpPost]
        public virtual ActionResult getDataWeight(string qr, string status)
        {
            try
            {
                var result = _serviceFulfill.GetDataByQR(qr).Result;
                decimal ret = _serviceFulfill.GetWeight().Result;

                if (result != null)
                {
                    if (status == "in")
                    {
                        if (result.doc_status != "NEW")
                        {
                            ret = result.weight_in;
                        }
                    }
                    else if (status == "out")
                    {
                        if (result.doc_status != "WEIGHT-IN")
                        {
                            ret = result.weight_out;
                        }
                    }
                    else if (status == "in_2")
                    {
                        if (result.doc_status != "WEIGHT-OUT")
                        {
                            ret = result.weight_in_2;
                        }
                    }
                    else
                    {
                        if (result.doc_status != "WEIGHT-IN-2")
                        {
                            ret = result.weight_out_2;
                        }
                    }
                }
                return Json(new { status = "success", model = ret });
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
            // Add 15 Feb cancle Scan gate 1 and 2 to sql gate control
            if (_gate.process != "SCAN_GATE_1" || _gate.process != "SCAN_GATE_2")
            {
                var _x = _serviceGate.OpenGate(_gate).Result;
                Thread.Sleep(10000);
                _x = _serviceGate.CloseGate(_gate).Result;
            }
          
        }

        [HttpPost]
        public virtual ActionResult GetPercentageMessage()
        {
            return Json(new { status = "success", message = _serviceInfo.GetPercentageMessage().Result });
        }

    }
}

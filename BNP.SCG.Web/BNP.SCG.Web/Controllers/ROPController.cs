using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    [Authorize]
    public class ROPController : Controller
    {
        private readonly ROPService _serviceROP;
        private readonly MasterDataService _serviceMaster;

        public ROPController(ROPService serviceROP, MasterDataService serviceMaster)
        {
            _serviceROP = serviceROP;
            _serviceMaster = serviceMaster;
        }

        public IActionResult Index()
        {
            return View();
        }

        public virtual ActionResult getDataROP()
        {
            var result = _serviceROP.GetAllROP().Result;
            return PartialView("_table", result);
        }

        public IActionResult FormROP(long id)
        {
            ViewBag.MaterialData = _serviceMaster.GetAllRawMaterials().Result.Where(w => w.is_active).ToList();
            ViewBag.LocationData = _serviceMaster.GetAllLocations().Result.Where(w => w.is_active).ToList();
            if (id > 0)
            {
                var data = _serviceROP.GetROPById(id).Result;
                if (data != null)
                {
                    return View(data);
                }
                return NotFound();
            }
            else
            {
                var data = new ROPData { id = id };
                return View(data);
            }
        }

        [HttpPost]
        public IActionResult FormROP(ROPData model)
        {
            ViewBag.MaterialData = _serviceMaster.GetAllRawMaterials().Result.Where(w => w.is_active).ToList();
            ViewBag.LocationData = _serviceMaster.GetAllLocations().Result.Where(w => w.is_active).ToList();
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                model.updated_at = DateTime.Now;
                var result = _serviceROP.EditROP(model).Result;
                if (!result.isCompleted)
                {
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
            }
            else
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.created_by = first_name;
                model.updated_by = first_name;
                model.created_at = DateTime.Now;
                model.updated_at = DateTime.Now;
                var result = _serviceROP.CreateROP(model).Result;
                if (!result.isCompleted)
                {
                    TempData["ErrorMessage"] = result.message[0];
                    return View(model);
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("Index", "ROP");
        }

        [HttpPost]
        public IActionResult DeleteROP(long id)
        {
            return Json(new { result = _serviceROP.DeleteROP(id).Result });
        }

        public IActionResult LoadAddon(long id, long location_id)
        {
            var _addon = _serviceROP.LoadAddOnROP(id).Result.Select(s => s.location_id).ToArray();
            var _loc = _serviceMaster.GetAllLocations().Result.Where(w => w.is_active && w.id != location_id).ToList();

            return Json(new { status = "success", addon = _addon, loc = _loc });
        }

        [HttpPost]
        public IActionResult SaveAddon(string locs)
        {
            try
            {
                var _locs = JsonConvert.DeserializeObject<List<ROPAddOnData>>(locs);
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                _locs.ForEach(x => x.created_by = first_name); _locs.ForEach(x => x.created_at = DateTime.Now);
                var result = _serviceROP.CreateEditROPAddon(_locs).Result;

                return Json(new { status = "success", data = result.data, message = result.message });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult DeleteAddon(string locs)
        {
            try
            {
                var _locs = JsonConvert.DeserializeObject<List<ROPAddOnData>>(locs);
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                _locs.ForEach(x => x.created_by = first_name); _locs.ForEach(x => x.created_at = DateTime.Now);
                var result = _serviceROP.DeleteROPAddon(_locs).Result;

                return Json(new { status = "success", data = result.data, message = result.message });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }

        }
    }
}

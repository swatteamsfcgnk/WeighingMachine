using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    [Authorize]
    public class MasterDataController : Controller
    {
        private readonly MasterDataService _masterData;
        private readonly ROPService _serviceROP;

        public MasterDataController(MasterDataService masterData, ROPService serviceROP)
        {
            _masterData = masterData;
            _serviceROP = serviceROP;

        }

        public IActionResult RawMaterial()
        {
            ViewBag.rawMaterials = _masterData.GetAllRawMaterials().Result;
            ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
            return View();
        }

        public IActionResult FormRawMaterial(int id)
        {
            if (id > 0)
            {
                var rawMaterial = _masterData.GetRawMaterialById(id).Result;

                if (rawMaterial != null)
                {
                    ViewBag.categories = _masterData.GetAllCategories().Result;
                    ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
                    ViewBag.vendors = _masterData.GetAllVendors().Result;
                    return View(rawMaterial);
                }

                return NotFound();
            }
            else
            {
                var rawMaterial = new RawMaterial { id = id };
                ViewBag.categories = _masterData.GetAllCategories().Result;
                ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
                ViewBag.vendors = _masterData.GetAllVendors().Result;
                return View(rawMaterial);
            }
        }

        [HttpPost]
        public IActionResult FormRawMaterial(RawMaterial model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.created_by = first_name;
                model.updated_by = first_name;
                var result = _masterData.EditRawMaterial(model).Result;
                if (!result.isCompleted)
                {
                    ViewBag.categories = _masterData.GetAllCategories().Result;
                    ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
                    ViewBag.vendors = _masterData.GetAllVendors().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                //var x = _serviceROP.DeleteROPByMatId(model.name).Result;
                TempData["Message"] = result.message[0];
            }
            else
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.created_by = first_name;
                model.updated_by = first_name;
                var result = _masterData.CreateRawMaterial(model).Result;
                if (!result.isCompleted)
                {
                    ViewBag.rawMaterials = _masterData.GetAllRawMaterials().Result;
                    ViewBag.categories = _masterData.GetAllCategories().Result;
                    ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
                    ViewBag.vendors = _masterData.GetAllVendors().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("RawMaterial", "MasterData");
        }

        [HttpPost]
        public IActionResult DeleteRawMaterial(int id)
        {
            var result = _masterData.DeleteRawMaterial(id).Result;

            if (result.isCompleted)
            {
                var x = _serviceROP.DeleteROPByMatId(id).Result;
            }

            return Json(new { result = _masterData.DeleteRawMaterial(id).Result });
        }

        public IActionResult Vendor()
        {
            ViewBag.vendors = _masterData.GetAllVendors().Result;
            return View();
        }

        public IActionResult FormVendor(int id)
        {
            if (id > 0)
            {
                var vendor = _masterData.GetVendorById(id).Result;
                if (vendor != null)
                {
                    return View(vendor);
                }
                return NotFound();
            }
            else
            {
                var vendor = new Vendor { id = id };
                return View(vendor);
            }
        }

        [HttpPost]
        public IActionResult FormVendor(Vendor model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                var result = _masterData.EditVendor(model).Result;
                if (!result.isCompleted)
                {
                    TempData["ErrorMessage"] = result.message[0];
                    return View(model);
                }
                TempData["Message"] = result.message[0];
            }
            else
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.created_by = first_name;
                model.updated_by = first_name;
                var result = _masterData.CreateVendor(model).Result;
                if (!result.isCompleted)
                {
                    var vendor = new Vendor { id = 0 };
                    TempData["ErrorMessage"] = result.message[0];
                    return View(vendor);
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("Vendor", "MasterData");
        }

        [HttpPost]
        public IActionResult DeleteVendor(int id)
        {
            return Json(new { result = _masterData.DeleteVendor(id).Result });
        }


        public IActionResult RawMaterialType()
        {
            ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
            return View();
        }

        public IActionResult FormRawMaterialType(int id)
        {
            if (id > 0)
            {
                var rawMaterialType = _masterData.GetRawMaterialTypeById(id).Result;
                if (rawMaterialType != null)
                {
                    return View(rawMaterialType);
                }
                return NotFound();
            }
            else
            {
                var rawMaterialType = new RawMaterialType { id = id, is_rop = true };
                return View(rawMaterialType);
            }

        }

        [HttpPost]
        public IActionResult FormRawMaterialType(RawMaterialType model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                var result = _masterData.EditRawMaterialType(model).Result;
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
                var result = _masterData.CreateRawMaterialType(model).Result;
                if (!result.isCompleted)
                {
                    ViewBag.rawMaterialTypes = _masterData.GetAllRawMaterialTypes().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("RawMaterialType", "MasterData");
        }

        [HttpPost]
        public IActionResult DeleteRawMaterialType(int id)
        {
            return Json(new { result = _masterData.DeleteRawMaterialType(id).Result });
        }

        public IActionResult Location()
        {
            ViewBag.locations = _masterData.GetAllLocations().Result;
            return View();
        }

        public IActionResult FormLocation(int id)
        {
            if (id > 0)
            {
                var location = _masterData.GetLocationById(id).Result;
                if (location != null)
                {
                    return View(location);
                }
                return NotFound();
            }
            else
            {
                var location = new Location { id = id };
                return View(location);
            }
        }

        [HttpPost]
        public IActionResult FormLocation(Location model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                var result = _masterData.EditLocation(model).Result;
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
                var result = _masterData.CreateLocation(model).Result;
                if (!result.isCompleted)
                {
                    ViewBag.locations = _masterData.GetAllLocations().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("Location", "MasterData");
        }

        [HttpPost]
        public IActionResult DeleteLocation(int id)
        {
            return Json(new { result = _masterData.DeleteLocation(id).Result });
        }

        public IActionResult Category()
        {
            ViewBag.categories = _masterData.GetAllCategories().Result;
            return View();
        }

        public IActionResult FormCategory(int id)
        {
            if (id > 0)
            {
                var category = _masterData.GetCategoryById(id).Result;
                if (category != null)
                {
                    return View(category);
                }
                return NotFound();
            }
            else
            {
                var category = new Category { id = id };
                return View(category);
            }
        }

        [HttpPost]
        public IActionResult FormCategory(Category model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                var result = _masterData.EditCategory(model).Result;
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
                var result = _masterData.CreateCategory(model).Result;
                if (!result.isCompleted)
                {
                    ViewBag.categories = _masterData.GetAllCategories().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("Category", "MasterData");
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            return Json(new { result = _masterData.DeleteCategory(id).Result });
        }

    }
}

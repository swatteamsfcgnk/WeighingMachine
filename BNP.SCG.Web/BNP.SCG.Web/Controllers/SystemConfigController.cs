using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    [Authorize]
    public class SystemConfigController : Controller
    {
        private readonly SystemConfigService _systemService;

        public SystemConfigController(SystemConfigService systemService)
        {
            _systemService = systemService;
        }

        public IActionResult Index()
        {
            var systemConfig = _systemService.GetSystemConfig().Result;
            return View(systemConfig);
        }

        [HttpPost]
        public IActionResult EditSystemConfig(SystemConfig model)
        {

            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
            model.updated_by = first_name;

            var result = _systemService.EditSystemConfig(model).Result;
            if (!result.isCompleted)
            {
                TempData["ErrorMessage"] = result.message[0];
                return View(model);
            }

            TempData["Message"] = result.message[0];
            return RedirectToAction("Index", "SystemConfig");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
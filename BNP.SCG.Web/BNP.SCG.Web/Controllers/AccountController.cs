using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BNP.SCG.Web.Controllers
{
    [Authorize(Roles = "admin, register, weight")]
    public class AccountController : Controller
    {
        private readonly AccountService _account;

        public AccountController(AccountService account)
        {
            _account = account;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewBag.errorMessage = null;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User model)
        {
            var user = _account.Login(model).Result;
            if (user != null)
            {
                var identity = new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Name, user.username),
                            new Claim("UserId", user.id.ToString()),
                            new Claim("UserFirstName", user.first_name),
                            new Claim("UserLastName", user.last_name),
                            new Claim(ClaimTypes.Role, user.role)},
                CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);
                if (user.role == "admin")
                {
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
                    });
                }
                else
                {
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddYears(1)
                    });
                }
                

                TempData["Message"] = "เข้าสู่ระบบสำเร็จ!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.errorMessage = "ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง";
            return View();
        }

        [Authorize(Roles = "admin")]
        public IActionResult Index()
        {
            ViewBag.users = _account.GetAllUsers().Result;
            return View();
        }

        [Authorize(Roles = "admin")]
        public IActionResult FormUser(int id)
        {
            if (id > 0)
            {
                var user = _account.GetUserById(id).Result;

                if (user != null)
                {
                    return View(user);
                }

                return NotFound();
            }
            else
            {
                var user = new User { id = id };
                return View(user);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult FormUser(User model)
        {
            if (model.id > 0)
            {
                var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();
                model.updated_by = first_name;
                var result = _account.EditUser(model).Result;
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
                var result = _account.CreateUser(model).Result;
                if (!result.isCompleted)
                {
                    ViewBag.users = _account.GetAllUsers().Result;
                    TempData["ErrorMessage"] = result.message[0];
                    return View();
                }
                TempData["Message"] = result.message[0];
            }

            return RedirectToAction("Index", "Account");
        }


        [AllowAnonymous]
        public IActionResult Profile(int id)
        {
            var user_id = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserId").Select(c => c.Value).FirstOrDefault();

            if (Int32.Parse(user_id) != id)
            {
                return RedirectToAction("AccessDenied");
            }

            var user = _account.GetUserById(id).Result;
            if (user != null)
            {
                return View(user);
            }
            return NotFound();

        }

        [HttpPost]
        public IActionResult Profile(User model)
        {
            var user_id = Int32.Parse(((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserId").Select(c => c.Value).FirstOrDefault());

            if (model.id != user_id)
            {
                return RedirectToAction("AccessDenied");
            }

            var first_name = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == "UserFirstName").Select(c => c.Value).FirstOrDefault();

            model.updated_by = first_name;
            var result = _account.EditUser(model).Result;
            if (!result.isCompleted)
            {
                TempData["ErrorMessage"] = result.message[0];
            }
            TempData["Message"] = result.message[0];
            TempData["NotiMessageUpdatedUser"] = result.message[1];
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteUser(int id)
        {
            if (id == 1)
            {
                var result = new ReturnObject<long>();
                result.isCompleted = false;
                result.message.Add("ไม่สามารถลบแอดมินได้!");
                return Json(new { result = result });
            }

            return Json(new { result = _account.DeleteUser(id).Result });
        }
        [AllowAnonymous]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
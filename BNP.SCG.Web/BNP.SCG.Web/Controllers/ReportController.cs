using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BNP.SCG.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly FulfillService _fulfill;
        private readonly CustomerService _customer;
        private readonly IWebHostEnvironment _env;
        public ReportController(FulfillService fulfill, CustomerService customer
                               , IWebHostEnvironment env)
        {
            _fulfill = fulfill;
            _customer = customer;
            _env = env;
        }

        public IActionResult Index()
        {
            var data = _fulfill.GetAll().Result.OrderByDescending(o => o.date_in).ToList();
            return View(data);

        }

        public IActionResult Customer()
        {
            var data = _customer.GetAllCustomers().Result.OrderByDescending(o => o.created_at).ToList();
            return View(data);
        }


    }
}
using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChapeauPOS.Controllers
{
	public class HomeController : BaseController
	{
		private readonly ILogger<HomeController> _logger;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly PasswordHasher<string> _passwordHasher;

        public HomeController(ILogger<HomeController> logger, IEmployeeRepository employeeRepository)
		{
			_logger = logger;
            _employeeRepository = employeeRepository;
            _passwordHasher = new PasswordHasher<string>();
        }

		public IActionResult Index()
		{
			return View();
		}

		[HttpGet]
        public IActionResult Login(string errorMessage)
        {
            List<Employee> employees = _employeeRepository.GetAllEmployees();
            ViewBag.Employees = employees;
            ViewBag.ErrorMessage = errorMessage;
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            
            Employee employee = _employeeRepository.GetEmployeeByIdAndPassword(loginModel);
            if(employee.EmployeeId == 0)
			{
				string errorMessage = "Invalid Password entered Please try again";
				return RedirectToAction("Login", new { errorMessage });
			}
            else
            {
                HttpContext.Session.SetObject("LoggedInUser", employee);
            }
            
            switch(employee.Role)
                {
                case Roles.Manager:
                    return RedirectToAction("Index", "Tables");
                case Roles.Waiter:
                    return RedirectToAction("Index", "Tables");
                case Roles.Cook:
                    return RedirectToAction("KitchenRunningOrders", "KitchenBar");
                case Roles.Bartender:
                    return RedirectToAction("BarRunningOrders", "KitchenBar");
                }
                return View(loginModel);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("LoggedInUser");
            return RedirectToAction("Login", "Home");
        }
        [HttpPost]
        public IActionResult SetTheme(string? theme)
        {
            if (theme != null)
            {
                CookieOptions options = new CookieOptions()
                {
                    Expires = DateTime.Now.AddDays(5),
                    Path = "/",
                    Secure = false,
                    HttpOnly = true,
                    IsEssential = true
                };
                Response.Cookies.Append("PreferedTheme", theme, options);
            }
            return RedirectToAction("Login");
        }
        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}

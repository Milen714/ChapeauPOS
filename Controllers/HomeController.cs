using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                    Console.WriteLine("Manager logged in");
                    return RedirectToAction("Index", "Employees");
                case Roles.Waiter:
                    Console.WriteLine("Waiter logged in");
                    return RedirectToAction("Index", "Home");
                case Roles.Cook:
                    Console.WriteLine("Cook logged in");
                    return RedirectToAction("Index", "Home");
                case Roles.Bartender:
                    Console.WriteLine("Bartender logged in");
                    return RedirectToAction("Index", "Home");
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
                    Secure = true,
                    HttpOnly = true,
                    IsEssential = true
                };
                Response.Cookies.Append("PreferedTheme", theme, options);
            }
            return RedirectToAction("Index", "Home");
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

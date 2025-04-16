using ChapeauPOS.Models;
using ChapeauPOS.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChapeauPOS.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly IEmployeeRepository _employeeRepository;

        public HomeController(ILogger<HomeController> logger, IEmployeeRepository employeeRepository)
		{
			_logger = logger;
            _employeeRepository = employeeRepository;
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
			Response.Cookies.Append("Role", employee.Role.ToString());
            Response.Cookies.Append("EmployeeID", employee.EmployeeId.ToString());
            if (employee.Role == (Roles)Enum.Parse(typeof(Roles), "Manager"))
			{
				Console.WriteLine("Manager logged in");
                return RedirectToAction("Index", "Employees");
            }
            else if (employee.Role == (Roles)Enum.Parse(typeof(Roles), "Waiter"))
			{
                Console.WriteLine("Waiter logged in");
            }
            else if (employee.Role == (Roles)Enum.Parse(typeof(Roles), "Cook"))
            {
                Console.WriteLine("Cook logged in");
            }
            else if (employee.Role == (Roles)Enum.Parse(typeof(Roles), "Bartender"))
            {
                Console.WriteLine("Bartender logged in");
                return RedirectToAction("Index", "Home");
            }

                return View(loginModel);
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

using System.Diagnostics;
using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmployeesService _employeesService;
        private readonly PasswordHasher<string> _passwordHasher;

        public HomeController(ILogger<HomeController> logger, IEmployeesService employeesService)
        {
            _logger = logger;
            _employeesService = employeesService;
            _passwordHasher = new PasswordHasher<string>();
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load home page: {ex.Message}";
                return RedirectToAction(nameof(Error));
            }
        }

        [HttpGet]
        public IActionResult Login(string errorMessage)
        {
            try
            {
                var employees = _employeesService.GetAllEmployees();
                ViewBag.Employees = employees;
                ViewBag.ErrorMessage = errorMessage;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load login page: {ex.Message}";
                return RedirectToAction(nameof(Error));
            }
        }

        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            try
            {
                var employee = _employeesService.GetEmployeeByIdAndPassword(loginModel);
                if (employee.EmployeeId == 0)
                {
                    string errorMessage = "Invalid password entered. Please try again.";
                    return RedirectToAction("Login", new { errorMessage });
                }

                HttpContext.Session.SetObject("LoggedInUser", employee);

                switch (employee.Role)
                {
                    case Roles.Manager:
                    case Roles.Waiter:
                        return RedirectToAction("Index", "Tables");
                    case Roles.Cook:
                        return RedirectToAction("KitchenRunningOrders", "KitchenBar");
                    case Roles.Bartender:
                        return RedirectToAction("BarRunningOrders", "KitchenBar");
                    default:
                        TempData["Error"] = "User role is not recognized.";
                        return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Login failed: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Remove("LoggedInUser");
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Logout failed: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public IActionResult SetTheme(string? theme)
        {
            try
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to set theme: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load privacy policy: {ex.Message}";
                return RedirectToAction(nameof(Error));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

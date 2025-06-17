using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class EmployeesController : BaseController
    {
        private readonly IEmployeesService _employeesService;
        private readonly PasswordHasher<string> _passwordHasher;

        public EmployeesController(IEmployeesService employeesService)
        {
            _employeesService = employeesService;
            _passwordHasher = new PasswordHasher<string>();
        }



        //  Helper method to check if the logged-in user is a manager

        private bool IsManagerLoggedIn()
        {
            var user = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            return user != null && user.Role == Roles.Manager;
        }


        //  Read-Only View: Employee Directory

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Index()
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "You do not have permission to access this page.";
                HttpContext.Session.Remove("LoggedInUser");
                return RedirectToAction("Login", "Home");
            }

            var employees = _employeesService.GetAllEmployees();
            ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            return View("Index", employees); // Views/Employees/Index.cshtml
        }

        // Manage View (Edit/Add/Activate/Deactivate)
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Manage()
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "You do not have permission to access this page.";
                HttpContext.Session.Remove("LoggedInUser");
                return RedirectToAction("Login", "Home");
            }

            var employees = _employeesService.GetAllEmployees();
            ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            return View("Manage", employees); // Views/Employees/Manage.cshtml
        }

        // Add Employee (GET)
        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee()
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            var employee = new Employee();
            return View("AddNewEmployee", employee); // Views/Employees/AddNewEmployee.cshtml
        }

        //  Add Employee (POST)
        [HttpPost]

        [ValidateAntiForgeryToken]

        [SessionAuthorize(Roles.Manager)]

        public IActionResult AddNewEmployee(Employee employee)
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                employee.Password = _passwordHasher.HashPassword(employee.Email, employee.Password);
                _employeesService.AddEmployee(employee);
                TempData["SuccessMessage"] = "Employee added successfully!";
                return RedirectToAction(nameof(Manage));
            }

            return View("AddNewEmployee", employee);
        }


        //  Edit Employee (GET)

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Edit(int id)
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            var employee = _employeesService.GetEmployeeById(id);
            return View("EditEmployee", employee); // Views/Employees/EditEmployee.cshtml
        }

        //  Edit Employee (POST)
        [HttpPost]

        [ValidateAntiForgeryToken]

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Edit(Employee employee)

        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            ModelState.Remove(nameof(employee.Password)); // Skip password field unless updating

            if (ModelState.IsValid)
            {
                _employeesService.UpdateEmployee(employee);
                TempData["SuccessMessage"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Manage));
            }

            return View("EditEmployee", employee);
        }

        //  Activate
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Activate(int id)
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            _employeesService.ActivateEmployee(id);
            TempData["SuccessMessage"] = "Employee activated!";
            return RedirectToAction(nameof(Manage));
        }


        //  Deactivate


        [SessionAuthorize(Roles.Manager)]
        public IActionResult Deactivate(int id)
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            _employeesService.DeactivateEmployee(id);
            TempData["SuccessMessage"] = "Employee deactivated!";
            return RedirectToAction(nameof(Manage));
        }
    }
}

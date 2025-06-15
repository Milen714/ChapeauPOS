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

        // ✅ Helper method to check if the logged-in user is a manager
        private bool IsManagerLoggedIn()
        {
            var user = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            return user != null && user.Role == Roles.Manager;
        }

        public IActionResult Index()
        {
            // ✅ Check if the logged-in user has permission
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "You do not have permission to access this page.";
                return RedirectToAction("Login", "Home");
            }

            // Retrieve all employees from the repository
            List<Employee> employees = _employeesService.GetAllEmployees();

            // Set the logged-in employee in the view
            ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

            return View(employees);
        }

        public IActionResult AddNewEmployee()
        {
            // ✅ Check if the logged-in user has permission
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            // Return empty employee form
            Employee employee = new Employee();
            return View(employee);
        }

        [HttpPost]
        public IActionResult AddNewEmployee(Employee employee)
        {
            // ✅ Check if the logged-in user has permission
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                // 🔐 Hash the password before saving the employee
                employee.Password = _passwordHasher.HashPassword(employee.Email, employee.Password);

                _employeesService.AddEmployee(employee);

                TempData["SuccessMessage"] = "Employee added successfully!"; // ✅ Feedback message

                return RedirectToAction(nameof(Index)); // ✅ Safer redirect
            }

            return View(employee);
        }

        public IActionResult Edit(int id)
        {
            // ✅ Check if the logged-in user has permission
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            var employee = _employeesService.GetEmployeeById(id);
            return View("EditEmployee", employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEmployee(Employee employee)
        {
            // ✅ Check if the logged-in user has permission
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            ModelState.Remove(nameof(employee.Password)); // ✅ FIX: prevent password validation if not used in edit

            if (ModelState.IsValid)
            {
                _employeesService.UpdateEmployee(employee);
                TempData["SuccessMessage"] = "Employee updated successfully!"; // ✅ Feedback message
                return RedirectToAction(nameof(Index));
            }

            return View("EditEmployee", employee);
        }

        public IActionResult Activate(int id)
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            _employeesService.ActivateEmployee(id);
            TempData["SuccessMessage"] = "Employee activated!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Deactivate(int id)
        {
            if (!IsManagerLoggedIn())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Login", "Home");
            }

            _employeesService.DeactivateEmployee(id);
            TempData["SuccessMessage"] = "Employee deactivated!";
            return RedirectToAction(nameof(Index));
        }
    }
}

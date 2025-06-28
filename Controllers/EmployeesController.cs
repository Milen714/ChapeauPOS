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


        // Employee Directory (Read-Only)

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
            try
            {
                var employees = _employeesService.GetAllEmployees();
                ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
                return View("Index", employees);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load employee directory: {ex.Message}";
                return View("Index", new List<Employee>());
            }
        }

        // Manage Employees View
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Manage()
        {
            try
            {
                var employees = _employeesService.GetAllEmployees();
                ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
                return View("Manage", employees);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load manage view: {ex.Message}";
                return View("Manage", new List<Employee>());
            }
        }

        // Add Employee (GET)
        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee()
        {

            try
            {
                var employee = new Employee();
                return View("AddNewEmployee", employee);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to open Add New Employee form: {ex.Message}";
                return RedirectToAction(nameof(Manage));
            }

            var employee = new Employee();
            return View("AddNewEmployee", employee); // Views/Employees/AddNewEmployee.cshtml

        }

        // Add Employee (POST)
        [HttpPost]

        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee(Employee employee)
        {
            try
            {
                if (ModelState.IsValid)
                {//THIS IS WHY I NEED THE PASSWORD HASHER IN THE EMPLOYEE CONTROLLER!!!
                    employee.Password = _passwordHasher.HashPassword(employee.Email, employee.Password);
                    _employeesService.AddEmployee(employee);
                    TempData["SuccessMessage"] = "Employee added successfully!";
                    return RedirectToAction(nameof(Manage));
                }

                return View("AddNewEmployee", employee);
            }
            catch (Exception ex)

        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee(Employee employee)
        {
            if (ModelState.IsValid)

            {
                TempData["Error"] = $"Failed to add employee: {ex.Message}";
                return View("AddNewEmployee", employee);
            }
        }


        // Edit Employee (GET)




        [SessionAuthorize(Roles.Manager)]
        public IActionResult Edit(int id)
        {
            try


            {
                var employee = _employeesService.GetEmployeeById(id);

                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToAction("Manage");
                }

                return View("EditEmployee", employee); // Views/Employees/EditEmployee.cshtml
            }
            catch (Exception ex)

            {
                var employee = _employeesService.GetEmployeeById(id);
                if (employee == null)
                {
                    TempData["Error"] = $"Employee not found.";
                    return RedirectToAction(nameof(Manage));
                }

                return View("EditEmployee", employee);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to load edit form: {ex.Message}";
                return RedirectToAction(nameof(Manage));
            }

        }


        // Edit Employee (POST)
        [HttpPost]

        [ValidateAntiForgeryToken]

        [SessionAuthorize(Roles.Manager)]
        public IActionResult Edit(Employee employee)
        {
            try
            {
                ModelState.Remove(nameof(employee.Password)); // Skip password field unless updating

                if (ModelState.IsValid)
                {
                    _employeesService.UpdateEmployee(employee);
                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Manage));
                }

                return View("EditEmployee", employee);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update employee: {ex.Message}";
                return View("EditEmployee", employee);
            }
        }

        // Activate Employee
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Activate(int id)
        {
            try
            {
                _employeesService.ActivateEmployee(id);
                TempData["SuccessMessage"] = "Employee activated!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to activate employee: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage));
        }

        // Deactivate Employee
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Deactivate(int id)
        {
            try
            {
                _employeesService.DeactivateEmployee(id);
                TempData["SuccessMessage"] = "Employee deactivated!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to deactivate employee: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}

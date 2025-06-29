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

        // Employee Directory (Read-Only View)
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Index()
        {
            try
            {
                var employees = _employeesService.GetAllEmployees();
                ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
                return View("Index", employees);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the employee directory.";
                return RedirectToAction("Index", "Home");
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
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the employee list.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Add Employee (GET)
        [HttpGet]
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
        }
        // Add Employee (POST)
        [HttpPost]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee(Employee employee)
        {
            // Check if the email address already exists
            if (_employeesService.EmailAddressExists(employee.Email))
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View("AddNewEmployee", employee);
            }

            if (ModelState.IsValid)
            {
                // Hash the password using email as the salt
                employee.Password = _passwordHasher.HashPassword(employee.Email, employee.Password);

                // Save the new employee
                _employeesService.AddEmployee(employee);
                TempData["SuccessMessage"] = "Employee added successfully!";
                return RedirectToAction(nameof(Manage));
            }

            // If validation fails, return view with error messages
            return View("AddNewEmployee", employee);
        }


        // Edit Employee (GET)
        [HttpGet]
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

                return View("EditEmployee", employee);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while trying to load the employee.";
                return RedirectToAction("Manage");
            }
        }

        // Edit Employee (POST)
        [HttpPost]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult EditEmployee(Employee employee)
        {
            ModelState.Remove(nameof(employee.Password));

            if (ModelState.IsValid)
            {
                try
                {
                    _employeesService.UpdateEmployee(employee);
                    TempData["EmployeeSuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Manage));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the employee.";
                    return RedirectToAction(nameof(Manage));
                }
            }

            return View("EditEmployee", employee);
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
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while activating the employee.";
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
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deactivating the employee.";
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}

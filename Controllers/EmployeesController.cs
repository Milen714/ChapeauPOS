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

                // Return the Index view with the list of employees
                return View("Index", employees); // Views/Employees/Index.cshtml
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the employee directory.";
                return RedirectToAction("Index", "Home"); // Or a dedicated error page
            }
        }

        // Manage Employees View (Edit/Add/Activate/Deactivate)
        // Accessible only to logged-in users with the Manager role
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Manage()
        {
            try
            {
                var employees = _employeesService.GetAllEmployees();
                ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

                // Return the Manage view with the employee list
                return View("Manage", employees); // Views/Employees/Manage.cshtml
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the employee list.";
                return RedirectToAction("Index", "Home"); // Or a dedicated error page
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


        //  Add Employee (POST)
        [HttpPost]
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
            {
                TempData["Error"] = $"Failed to add employee: {ex.Message}";
                return View("AddNewEmployee", employee);
            }
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

                return View("EditEmployee", employee); // Views/Employees/EditEmployee.cshtml
            }
            catch (Exception ex)

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
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the employee.";
                    return RedirectToAction(nameof(Manage));
                }
            }

            return View("EditEmployee", employee);
        }


        // Activate Employee by ID
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
                TempData["ErrorMessage"] = "An error occurred while activating the employee.";
            }

            return RedirectToAction(nameof(Manage));
        }


        // Deactivate Employee by ID
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
                TempData["ErrorMessage"] = "An error occurred while deactivating the employee.";
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}

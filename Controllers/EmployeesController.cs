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
        //private bool IsManagerLoggedIn()
        //{
        //    var user = HttpContext.Session.GetObject<Employee>("LoggedInUser");
        //    return user != null && user.Role == Roles.Manager;
        //}

        // Employee Directory (Read-Only View)
        // Accessible only to logged-in users with the Manager role
        [SessionAuthorize(Roles.Manager)]
        public IActionResult Index()
        {
            try
            {
                // Retrieve all employees to display in the read-only Index view
                var employees = _employeesService.GetAllEmployees();

                // Pass the currently logged-in employee info to the view ( for displaying in header)
                ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

                // Return the Index view with the list of employees
                return View("Index", employees); // Views/Employees/Index.cshtml
            }
            catch (Exception ex)
            {
                //  log error (Failed to load employee directory)
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
                // Get all employees to display in the Manage view
                var employees = _employeesService.GetAllEmployees();

                // Pass logged-in manager's info to the view (for header display)
                ViewBag.LoggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

                // Return the Manage view with the employee list
                return View("Manage", employees); // Views/Employees/Manage.cshtml
            }
            catch (Exception ex)
            {
                //  log error 
                TempData["ErrorMessage"] = "An error occurred while loading the employee list.";
                return RedirectToAction("Index", "Home"); // Or a dedicated error page
            }
        }


        // Add Employee (GET)
        [HttpGet]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee()
        {
            var employee = new Employee();
            return View("AddNewEmployee", employee);
        }


        //  Add Employee (POST)
        [HttpPost]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult AddNewEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.Password = _passwordHasher.HashPassword(employee.Email, employee.Password);
                _employeesService.AddEmployee(employee);
                TempData["SuccessMessage"] = "Employee added successfully!";
                return RedirectToAction(nameof(Manage));
            }

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

                return View("EditEmployee", employee); // Views/Employees/EditEmployee.cshtml
            }
            catch (Exception ex)
            {
                // Log the error if you have logging set up (optional)
                TempData["ErrorMessage"] = "An error occurred while trying to load the employee.";
                return RedirectToAction("Manage");
            }
        }

        // Edit Employee (POST)
        // Handles form submission to update an existing employee
        [HttpPost]
        [SessionAuthorize(Roles.Manager)]
        public IActionResult EditEmployee(Employee employee)
        {
            // Exclude password from validation during edit unless it's explicitly being updated
            ModelState.Remove(nameof(employee.Password));

            if (ModelState.IsValid)
            {
                try
                {
                    _employeesService.UpdateEmployee(employee);
                    //TempData["SuccessMessage"] = "Employee updated successfully!";
                    TempData["EmployeeSuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Manage));
                }
                catch (Exception ex)
                {
                    // Optional: log the exception
                    TempData["ErrorMessage"] = "An error occurred while updating the employee.";
                    return RedirectToAction(nameof(Manage));
                }
            }

            // If validation fails, return the same view with validation messages
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
                // Optional: log the exception (e.g., _logger.LogError(ex, ...))
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
                //log the error(ex, "Failed to deactivate employee with ID: " + id))
                TempData["ErrorMessage"] = "An error occurred while deactivating the employee.";
            }

            return RedirectToAction(nameof(Manage));
        }

    }
}

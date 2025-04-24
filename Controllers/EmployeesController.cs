using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class EmployeesController : BaseController
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly PasswordHasher<string> _passwordHasher;
        public EmployeesController(IEmployeeRepository employeesRepository)
        {
            _employeeRepository = employeesRepository;
            _passwordHasher = new PasswordHasher<string>();
        }
        public IActionResult Index()
        {
            // Retrieve all employees from the repository
            List<Employee> employees = _employeeRepository.GetAllEmployees();

            Employee? loggedInEmployee = new Employee();
            loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            
            if (loggedInEmployee == null || loggedInEmployee.Role != Roles.Manager)
            {
                TempData["ErrorMessage"] = "You do not have permission to access this page.";
                return RedirectToAction("Login", "Home");
            }
            ViewBag.LoggedInEmployee = loggedInEmployee;
            return View(employees);
        }
        public IActionResult AddNewEmployee()
        {
            Employee employee = new Employee();
            return View(employee);
        }
        [HttpPost]
        public IActionResult AddNewEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                

                _employeeRepository.AddEmployee(employee);
                // Save the new employee to the repository
                //_employeeRepository.AddEmployee(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

    }
}

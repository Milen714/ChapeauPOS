using ChapeauPOS.Models;
using ChapeauPOS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        public EmployeesController(IEmployeeRepository employeesRepository)
        {
            _employeeRepository = employeesRepository;
        }
        public IActionResult Index()
        {
            // Retrieve all employees from the repository
            List<Employee> employees = _employeeRepository.GetAllEmployees();

            string? employeeId = Request.Cookies["EmployeeID"];
            string? role = Request.Cookies["Role"];
            if (employeeId == null || role == null)
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.LoggedInEmployee = employeeId;
            ViewBag.Role = role;
            return View(employees);
        }
    }
}

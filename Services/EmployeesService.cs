using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class EmployeesService : IEmployeesService
    {
        private readonly IEmployeeRepository _employeeRepository;
        public EmployeesService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        public Employee GetEmployeeByIdAndPassword(LoginModel loginModel)
        {
            return _employeeRepository.GetEmployeeByIdAndPassword(loginModel);
        }
        public List<Employee> GetAllEmployees()
        {
            return _employeeRepository.GetAllEmployees();
        }
        public void UpdateEmployee(Employee employee)
        {
            _employeeRepository.UpdateEmployee(employee);
        }
        public void AddEmployee(Employee employee)
        {
            _employeeRepository.AddEmployee(employee);
        }

        public Employee GetEmployeeById(int id)
        {
            return _employeeRepository.GetEmployeeById(id);
        }
        public void ActivateEmployee(int id)
        {
            _employeeRepository.ActivateEmployee(id);
        }

        public void DeactivateEmployee(int id)
        {
            _employeeRepository.DeactivateEmployee(id);
        }

        public bool EmailAddressExists(string email)
        {
            return _employeeRepository.EmailAddressExists(email);
        }


    }
}

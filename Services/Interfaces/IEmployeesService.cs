using ChapeauPOS.Models;

namespace ChapeauPOS.Services.Interfaces
{
    public interface IEmployeesService
    {
        Employee GetEmployeeByIdAndPassword(LoginModel loginModel);
        List<Employee> GetAllEmployees();
        void UpdateEmployee(Employee employee);
        void AddEmployee(Employee employee);
    }
}

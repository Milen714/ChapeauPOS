using ChapeauPOS.Models;

namespace ChapeauPOS.Services.Interfaces
{
    public interface IEmployeesService
    {
        Employee GetEmployeeByIdAndPassword(LoginModel loginModel);
        List<Employee> GetAllEmployees();
        void UpdateEmployee(Employee employee);
        void AddEmployee(Employee employee);
        Employee GetEmployeeById(int id);
        void ActivateEmployee(int id);
        void DeactivateEmployee(int id);
        bool EmailAddressExists(string email);

    }
}

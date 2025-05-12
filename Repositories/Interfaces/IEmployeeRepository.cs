using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
       
        Employee GetEmployeeByIdAndPassword(LoginModel loginModel);
        List<Employee> GetAllEmployees();
        
        void UpdateEmployee(Employee employee);
        void AddEmployee(Employee employee);

    }
}

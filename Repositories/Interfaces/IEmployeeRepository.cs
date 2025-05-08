using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Gets the employee by username and password.
        /// </summary>
        /// <param name="loginModel">The login model containing the username and password.</param>
        /// <returns>The employee if found; otherwise, null.</returns>
        Employee GetEmployeeByIdAndPassword(LoginModel loginModel);

        /// <summary>
        /// Gets all employees.
        /// </summary>
        /// <returns>A list of all employees.</returns>
        List<Employee> GetAllEmployees();

        /// <summary>
        /// Updates the employee information.
        /// </summary>
        /// <param name="employee">The employee to update.</param>
        void UpdateEmployee(Employee employee);
        void AddEmployee(Employee employee);

    }
}

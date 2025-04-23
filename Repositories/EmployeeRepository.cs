using ChapeauPOS.Models;
using Microsoft.Data.SqlClient;

namespace ChapeauPOS.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string? _connectionString;
        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }
        private Employee ReadEmployee(SqlDataReader reader)
        {
            int employeeId = reader.GetInt32(0);
            string firstName = reader.GetString(1);
            string lastName = reader.GetString(2);
            string password = reader.GetString(3);
            string email = reader.GetString(4);
            Roles role = (Roles)Enum.Parse(typeof(Roles), reader.GetString(5));
            return new Employee(employeeId, firstName, lastName, password, email, role);
        }
        List<Employee> IEmployeeRepository.GetAllEmployees()
        {
            List<Employee> employees = new List<Employee>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT EmployeeID, FirstName, LastName, Password, Email, Role FROM Employees";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Employee employee = ReadEmployee(reader);
                    employees.Add(employee);
                }
            }
            return employees;
        }

        Employee IEmployeeRepository.GetEmployeeByIdAndPassword(LoginModel loginModel)
        {
            Employee employee = new Employee();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = " SELECT EmployeeID, FirstName, LastName, Password, Email, Role " +
                               " FROM Employees " +
                               " WHERE EmployeeID = @EmployeeID AND Password = @Password ";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", loginModel.EmployeeID);
                command.Parameters.AddWithValue("@Password", loginModel.Password);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    employee = ReadEmployee(reader);
                }
                reader.Close();
            }
            return employee;
        }

        void IEmployeeRepository.UpdateEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }
    }
}

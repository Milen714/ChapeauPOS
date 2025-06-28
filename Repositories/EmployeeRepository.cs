using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
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

        // Helper method to read employee data from SqlDataReader
        private Employee ReadEmployee(SqlDataReader reader)
        {
            int employeeId = reader.GetInt32(0);
            string firstName = reader.GetString(1);
            string lastName = reader.GetString(2);
            string password = reader.GetString(3);
            string email = reader.GetString(4);
            Roles role = (Roles)Enum.Parse(typeof(Roles), reader.GetString(5));
            EmployeeGender gender = (EmployeeGender)Enum.Parse(typeof(EmployeeGender), reader.GetString(6));
            bool isActive = reader.GetBoolean(7);

            return new Employee(employeeId, firstName, lastName, password, email, role, gender, isActive);
        }

        public List<Employee> GetAllEmployees()
        {
            List<Employee> employees = new List<Employee>();

            if (string.IsNullOrEmpty(_connectionString)) return employees;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();


                //string query = "SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender, IsActive " +
                //               " FROM Employees " +
                //               " WHERE IsActive = 1; ";
                string query = "SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender, IsActive FROM Employees;";

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

        public Employee GetEmployeeByIdAndPassword(LoginModel loginModel)
        {
            Employee employee = new Employee();

            if (string.IsNullOrEmpty(_connectionString)) return employee;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender, IsActive " +
                               "FROM Employees WHERE EmployeeID = @EmployeeID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", loginModel.EmployeeID);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    employee = ReadEmployee(reader);

                    var hasher = new PasswordHasher<string>();
                    var result = hasher.VerifyHashedPassword(null, employee.Password, loginModel.Password);

                    if (result == PasswordVerificationResult.Failed)
                    {
                        employee.EmployeeId = 0; // Invalid login
                    }
                }

                reader.Close();
            }
            return employee;
        }

        public Employee GetEmployeeById(int id)
        {
            Employee employee = new Employee();

            if (string.IsNullOrEmpty(_connectionString)) return employee;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender, IsActive " +
                                   "FROM Employees WHERE EmployeeID = @EmployeeID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmployeeID", id);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        employee = ReadEmployee(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching employee by ID: " + ex.Message);
            }

            return employee;
        }

        public void AddEmployee(Employee employee)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            var hasher = new PasswordHasher<string>();
            string hashedPassword = hasher.HashPassword(null, employee.Password);

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Employees (FirstName, LastName, Password, Email, Role, Gender, IsActive) " +
                                   "VALUES (@FirstName, @LastName, @Password, @Email, @Role, @Gender, @IsActive)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Role", employee.Role.ToString());
                    command.Parameters.AddWithValue("@Gender", employee.Gender.ToString());
                    command.Parameters.AddWithValue("@IsActive", employee.IsActive);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding employee: " + ex.Message);
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Email = @Email, " +
                                   "Role = @Role, Gender = @Gender WHERE EmployeeID = @EmployeeID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Role", employee.Role.ToString());
                    command.Parameters.AddWithValue("@Gender", employee.Gender.ToString());
                    command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeId);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating employee: " + ex.Message);
            }
        }

        public void ActivateEmployee(int id)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "UPDATE Employees SET IsActive = 1 WHERE EmployeeID = @EmployeeID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error activating employee: " + ex.Message);
            }
        }

        public void DeactivateEmployee(int id)
        {
            if (string.IsNullOrEmpty(_connectionString)) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "UPDATE Employees SET IsActive = 0 WHERE EmployeeID = @EmployeeID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deactivating employee: " + ex.Message);
            }
        }
    }
}

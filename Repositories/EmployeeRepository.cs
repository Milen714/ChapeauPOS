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
        private Employee ReadEmployee(SqlDataReader reader)
        {
            int employeeId = reader.GetInt32(0);
            string firstName = reader.GetString(1);
            string lastName = reader.GetString(2);
            string password = reader.GetString(3);
            string email = reader.GetString(4);
            Roles role = (Roles)Enum.Parse(typeof(Roles), reader.GetString(5));
            EmployeeGender gender = (EmployeeGender)Enum.Parse(typeof(EmployeeGender), reader.GetString(6));
            return new Employee(employeeId, firstName, lastName, password, email, role, gender);
        }
        List<Employee> IEmployeeRepository.GetAllEmployees()
        {
            List<Employee> employees = new List<Employee>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender FROM Employees";
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
                string query = " SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender " +
                               " FROM Employees " +
                               " WHERE EmployeeID = @EmployeeID ";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", loginModel.EmployeeID);
                //command.Parameters.AddWithValue("@Password", loginModel.Password);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // Read the employee data from the database
                    // The password is hashed, so we need to verify it using PasswordHasher
                    employee = ReadEmployee(reader);
                    string storedHashedPassword = employee.Password;
                    var hasher = new PasswordHasher<string>();
                    var result = hasher.VerifyHashedPassword(null, storedHashedPassword, loginModel.Password);

                    if (result == PasswordVerificationResult.Failed)
                    {
                        employee.EmployeeId = 0; // Set to 0 if password verification fails
                    }
                    else
                    {
                        employee.Password = storedHashedPassword; // Keep the hashed password
                    }
                }
                reader.Close();
            }
            return employee;
        }

        

        void IEmployeeRepository.UpdateEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }

        void IEmployeeRepository.AddEmployee(Employee employee)
        {
            // Hash the password before storing it
            var hasher = new PasswordHasher<string>();
            string hashedPassword = hasher.HashPassword(null, employee.Password);
            try
            {
                using (SqlConnection connection = new SqlConnection (_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Employees (FirstName, LastName, Password, Email, Role, Gender) " +
                                   "VALUES (@FirstName, @LastName, @Password, @Email, @Role, @Gender)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Role", employee.Role.ToString());
                    command.Parameters.AddWithValue("@Gender", employee.Gender.ToString());
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public Employee GetEmployeeById(int id)
        {
            Employee employee = new Employee();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = " SELECT EmployeeID, FirstName, LastName, Password, Email, Role, Gender " +
                                " FROM Employees " +
                                " WHERE EmployeeID = @EmployeeID ";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EmployeeID", id);
                    command.Connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        employee = ReadEmployee(reader);
                    }

                }

            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return employee;
        }
    }
}

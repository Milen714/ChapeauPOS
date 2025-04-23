namespace ChapeauPOS.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Roles Role { get; set; }

        public Employee(int employeeId, string firstName, string lastName, string password, string email, Roles role)
        {
            EmployeeId = employeeId;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            Email = email;
            Role = role;
        }

        public Employee()
        {
        }
    }
}

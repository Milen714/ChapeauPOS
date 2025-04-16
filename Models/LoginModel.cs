namespace ChapeauPOS.Models
{
    public class LoginModel
    {
        public int EmployeeID { get; set; }
        public string Password { get; set; }
        

        public LoginModel(int employeeId, string password)
        {
            EmployeeID = employeeId;
            Password = password;
            
        }

        public LoginModel()
        {
        }
    }
}

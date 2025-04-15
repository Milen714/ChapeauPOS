namespace ChapeauPOS.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        public LoginModel(string username, string password, bool rememberMe)
        {
            UserName = username;
            Password = password;
            RememberMe = rememberMe;
        }

        public LoginModel()
        {
        }
    }
}

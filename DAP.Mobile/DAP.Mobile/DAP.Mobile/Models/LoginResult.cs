namespace DAP.Mobile.Models
{
    public class LoginResult
    {
        public string Token { get; set; }
        public string Error { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}

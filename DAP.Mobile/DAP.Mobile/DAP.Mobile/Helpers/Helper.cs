using System.Text.RegularExpressions;

namespace DAP.Mobile.Helpers
{
    public static class Helper
    {
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }
    }
}

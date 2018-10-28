using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace DAP.Mobile.Helpers
{
    public static class Helper
    {
        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }

        public static T GetApplicationValue<T>(string key)
        {
            if (Application.Current.Properties.ContainsKey(key))
            {
                return (T)Application.Current.Properties[key];
            }
            else
            {
                return default(T);
            }
        }

        public static void SetApplicationValue<T>(string key, T value)
        {
            if (Application.Current.Properties.ContainsKey(key))
            {
                Application.Current.Properties[key] = value;
            }
            else
            {
                Application.Current.Properties.Add(key, value);
            }
        }
    }
}
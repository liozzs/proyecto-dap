using DAP.Mobile.Helpers;

namespace DAP.Mobile
{
    public static class GlobalVariables
    {
        public static string BaseUrlApi = "http://ec2-18-219-97-48.us-east-2.compute.amazonaws.com:50065";
        public static string BaseUrlArduino = Helper.GetApplicationValue<string>("ArduinoIP");
    }
}
using DAP.Mobile.Services;
using EspTouchMultiPlatformLIbrary;
using Xamarin.Forms;

[assembly: Dependency(typeof(DAP.Mobile.Droid.SmartConfig))]
namespace DAP.Mobile.Droid
{
    public class SmartConfig : ISmartConfigHelper
    {
        public SmartConfig()
        {
        }

        public ISmartConfigTask CreatePlatformTask()
        {
            return new SmartConfigTask_Droid();
        }
    }
}
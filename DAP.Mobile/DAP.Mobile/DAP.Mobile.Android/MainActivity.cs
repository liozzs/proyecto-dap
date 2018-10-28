using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using DAP.Mobile.Views;
using Prism;
using Prism.Ioc;
using System;
using System.Linq;

namespace DAP.Mobile.Droid
{
    [Activity(Label = "DAP", Icon = "@drawable/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        long lastPress;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));
        }

        public override void OnBackPressed()
        {
            var actionPage = App.Current.MainPage;
            if (actionPage is MenuPage m && m.Detail.Navigation != null && m.Detail.Navigation.NavigationStack.Count == 1)
            {
                long currentTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

                if (currentTime - lastPress > 5000)
                {
                    Toast.MakeText(this, "Presione atrás otra vez para salir", ToastLength.Long).Show();
                    lastPress = currentTime;
                    return;
                }
            }

            base.OnBackPressed();
        }

        public class AndroidInitializer : IPlatformInitializer
        {
            public void RegisterTypes(IContainerRegistry container)
            {
                // Register any platform specific implementations
            }
        }
    }
}
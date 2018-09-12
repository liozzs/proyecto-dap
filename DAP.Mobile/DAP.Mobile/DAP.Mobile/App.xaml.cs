using DAP.Mobile.Services;
using DAP.Mobile.Views;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace DAP.Mobile
{
    public partial class App : PrismApplication
    {
        /*
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor.
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */

        public App() : this(null)
        {
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {
        }

        protected override void OnInitialized()
        {
            InitializeComponent();

            NavigationService.NavigateAsync("NavigationPage/LoginPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IApiClient>(new ApiClient());
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage>();
            containerRegistry.RegisterForNavigation<SignUpPage>();
            containerRegistry.RegisterForNavigation<ResetPasswordPage>();
            containerRegistry.RegisterForNavigation<MenuPage>();
            containerRegistry.RegisterForNavigation<MenuDetailPage>();
            containerRegistry.RegisterForNavigation<NotificationsPage>();
            containerRegistry.RegisterForNavigation<PlanificationPage>();
            containerRegistry.RegisterForNavigation<DailyPlanificationPage>();
            containerRegistry.RegisterForNavigation<WeeklyPlanificationPage>();
            containerRegistry.RegisterForNavigation<CustomPlanificationPage>();
            containerRegistry.RegisterForNavigation<PlanificationActionPage>();
            containerRegistry.RegisterForNavigation<ConfigurationPage>();
            containerRegistry.RegisterForNavigation<LoadPillsPage>();
        }
    }
}
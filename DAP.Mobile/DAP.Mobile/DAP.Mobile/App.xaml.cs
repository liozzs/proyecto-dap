using DAP.Mobile.Helpers;
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

        static SqliteService database;
        
        public static SqliteService Database
        {
            get
            {
                if (database == null)
                {
                    database = new SqliteService();
                }
                return database;
            }
        }

        public App() : this(null)
        {
            var ip = Helper.GetApplicationValue<string>("ArduinoIP");
            Helper.SetApplicationValue("ArduinoIP", $"http://{ip}");
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {
        }

        protected override void OnInitialized()
        {
            InitializeComponent();

            if (Helper.GetApplicationValue<bool>("logged"))
            {
                NavigationService.NavigateAsync("/MenuPage/NavigationPage/MenuDetailPage");
            }
            else
            {
                NavigationService.NavigateAsync("NavigationPage/LoginPage");
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IApiClient>(new ApiClient());
            containerRegistry.RegisterSingleton<ISqliteService, SqliteService>();
            containerRegistry.RegisterForNavigation<Views.NavigationPage>();
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
            containerRegistry.RegisterForNavigation<PillListPage>();
            containerRegistry.RegisterForNavigation<LoadPillsPage>();
            containerRegistry.RegisterForNavigation<PlanificationListPage>();
        }
    }
}
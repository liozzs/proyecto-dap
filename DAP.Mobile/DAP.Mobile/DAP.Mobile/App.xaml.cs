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
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();

            this.NavigationService.NavigateAsync("MainPage/LoginPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<LoginPage>();
            containerRegistry.RegisterForNavigation<SignUpPage>();
            containerRegistry.RegisterForNavigation<ResetPasswordPage>();
            containerRegistry.RegisterForNavigation<MenuPage>();
            containerRegistry.RegisterForNavigation<NotificationsPage>();
            containerRegistry.RegisterForNavigation<PlanificationPage>();
            containerRegistry.RegisterForNavigation<ConfigurationPage>();
            containerRegistry.RegisterForNavigation<LoadPillsPage>();
        }
    }
}

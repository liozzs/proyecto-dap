using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class MenuPageViewModel : ViewModelBase
    {
        public ObservableCollection<MasterPageItem> MenuItems { get; set; }

        public DelegateCommand<object> NavigateToCommand { get; set; }

        public MenuPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            NavigateToCommand = new DelegateCommand<object>(async (obj) => await NavigateTo(obj));
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            MenuItems = new ObservableCollection<MasterPageItem>();

            MenuItems.Add(new MasterPageItem()
            {
                Title = "Notificaciones",
                IconSource = "notifications.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Notifications))
            });
            MenuItems.Add(new MasterPageItem()
            {
                Title = "Planificación",
                IconSource = "planification.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Planification))
            });
            MenuItems.Add(new MasterPageItem()
            {
                Title = "Configuración",
                IconSource = "configuration.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Configuration))
            });
            MenuItems.Add(new MasterPageItem()
            {
                Title = "Cargar pastillas",
                IconSource = "loadPills.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Notifications))
            });
            MenuItems.Add(new MasterPageItem()
            {
                Title = "Cerrar sesión",
                IconSource = "signOut.png",
                Command = new DelegateCommand(async () => await SignOut())
            });
        }

        private Task SignOut()
        {
            return NavigationService.NavigateAsync("/NavigationPage/LoginPage");
        }

        private Task NavigateTo(object obj)
        {
            MenuNavigation menuNavigation = (MenuNavigation)obj;
            return NavigationService.NavigateAsync($"/MenuPage/NavigationPage/MenuDetailPage/{ menuNavigation.ToString()}Page");
        }
    }

    public class MasterPageItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
        public DelegateCommand Command { get; set; }
    }
}

using DAP.Mobile.Helpers;
using DAP.Mobile.Models;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class MenuPageViewModel : ViewModelBase
    {
        public ObservableCollection<MenuItem> MenuItems { get; set; }

        public DelegateCommand<object> NavigateToCommand { get; set; }

        public MenuPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            NavigateToCommand = new DelegateCommand<object>(async (obj) => await NavigateTo(obj));
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItem>();

            MenuItems.Add(new MenuItem()
            {
                Title = "Notificaciones",
                IconSource = "notifications.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Notifications))
            });
            MenuItems.Add(new MenuItem()
            {
                Title = "Planificación",
                IconSource = "planification.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Planification))
            });
            MenuItems.Add(new MenuItem()
            {
                Title = "Configuración",
                IconSource = "configuration.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Configuration))
            });
            MenuItems.Add(new MenuItem()
            {
                Title = "Cargar pastillas",
                IconSource = "loadPills.png",
                Command = new DelegateCommand(async () => await NavigateTo(MenuNavigation.Notifications))
            });
            MenuItems.Add(new MenuItem()
            {
                Title = "Cerrar sesión",
                IconSource = "signOut.png",
                Command = new DelegateCommand(async () => await SignOut())
            });
        }

        private Task SignOut()
        {
            Helper.SetApplicationValue("user", "");
            Helper.SetApplicationValue("logged", false);
            return NavigationService.NavigateAsync("/NavigationPage/LoginPage");
        }

        private Task NavigateTo(object obj)
        {
            MenuNavigation menuNavigation = (MenuNavigation)obj;
            return NavigationService.NavigateAsync($"/MenuPage/NavigationPage/MenuDetailPage/{ menuNavigation.ToString()}Page");
        }
    }
}
using Prism.Commands;
using Prism.Navigation;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class MenuDetailPageViewModel : ViewModelBase
    {
        public DelegateCommand<object> NavigateToCommand { get; set; }

        public MenuDetailPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            NavigateToCommand = new DelegateCommand<object>(async (obj) => await NavigateTo(obj));
        }

        private Task NavigateTo(object obj)
        {
            MenuNavigation menuNavigation = (MenuNavigation)obj;
            return NavigationService.NavigateAsync($"{menuNavigation.ToString()}Page");
        }
    }
}
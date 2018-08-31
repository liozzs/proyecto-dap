using Prism.Commands;
using Prism.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class LoadPillsPageViewModel : ViewModelBase
	{
        public string PillName { get; set; }
        public int? Quantity { get; set; }
        public int Container { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public LoadPillsPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await Accept());
        }

        private async Task Accept()
        {
            await NavigationService.GoBackAsync();
        }
    }
}

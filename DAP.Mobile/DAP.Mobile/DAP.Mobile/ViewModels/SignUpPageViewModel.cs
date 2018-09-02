using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class SignUpPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string ConfirmEmail { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public SignUpPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;

            CancelCommand = new Command(async () => await navigationService.GoBackAsync());
            AcceptCommand = new Command(async () => await Accept());
        }

        private async Task Accept()
        {
            await NavigationService.GoBackAsync();
            await dialogService.DisplayAlertAsync("Registro", "Todo piols", "Aceptar");
        }
    }
}

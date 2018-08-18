using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class ResetPasswordPageViewModel : ViewModelBase
	{
        private readonly INavigationService navigationService;
        private readonly IPageDialogService dialogService;

        private string message;

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }
        
        public string Email { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public ResetPasswordPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            CancelCommand = new Command(async () => await navigationService.GoBackAsync());
            AcceptCommand = new Command(async () => await Accept());
        }

        private async Task Accept()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Ingresá tu email";
            }
            else
            {
                await navigationService.GoBackAsync();
                await dialogService.DisplayAlertAsync("Recuperar contraseña", "Enviamos un mail a tu casilla para que recuperes tu contraseña", "Aceptar");
            }
        }
    }
}

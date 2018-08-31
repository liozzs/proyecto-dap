using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class ResetPasswordPageViewModel : ViewModelBase
	{
        private readonly IPageDialogService dialogService;
        
        public string Email { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public ResetPasswordPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;

            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await Accept());
        }

        private async Task Accept()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Ingresá tu email";
            }
            else
            {
                await NavigationService.GoBackAsync();
                await dialogService.DisplayAlertAsync("Recuperar contraseña", "Enviamos un mail a tu casilla para que recuperes tu contraseña", "Aceptar");
            }
        }
    }
}

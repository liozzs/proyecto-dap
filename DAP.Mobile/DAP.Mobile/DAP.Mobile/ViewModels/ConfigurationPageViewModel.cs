using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class ConfigurationPageViewModel : ViewModelBase
    {
        public string ActualPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public ConfigurationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;
            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            if (string.IsNullOrWhiteSpace(ActualPassword))
            {
                Message = "Ingresá tu password actual";
            }
            else
            {
                await NavigationService.GoBackAsync();
                await dialogService.DisplayAlertAsync("Recuperar contraseña", "Enviamos un mail a tu casilla para que recuperes tu contraseña", "Aceptar");
            }
        }
    }
}
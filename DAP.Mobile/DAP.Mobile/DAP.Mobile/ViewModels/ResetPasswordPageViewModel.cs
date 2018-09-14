using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class ResetPasswordPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public string Email { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public ResetPasswordPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient) : base(navigationService)
        {
            this.apiClient = apiClient;
            this.dialogService = dialogService;

            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await Accept());
        }

        private async Task Accept()
        {
            try
            {
                if (Validate())
                {
                    ApiClientOption option = new ApiClientOption
                    {
                        RequestType = ApiClientRequestTypes.Post,
                        Uri = "api/register",
                        BaseUrl = GlobalVariables.BaseUrlApi,
                        RequestContent = new { Email }
                    };

                    await apiClient.InvokeDataServiceAsync(option);

                    await dialogService.DisplayAlertAsync("Recuperar contraseña", "Enviamos un mail a su casilla para que recupere su contraseña", "Aceptar");

                    await NavigationService.GoBackAsync();
                }
            }
            catch
            {
                await dialogService.DisplayAlertAsync("Recuperar contraseña", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
            }
        }

        private bool Validate()
        {
            Message = null;

            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Ingrese su email";
            }
            else if (!Helpers.Helper.IsValidEmail(Email))
            {
                Message = "El email ingresado es inválido";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
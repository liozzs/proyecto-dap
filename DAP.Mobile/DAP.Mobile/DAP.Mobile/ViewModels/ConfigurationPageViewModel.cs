using DAP.Mobile.Helpers;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class ConfigurationPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public string ActualPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public ConfigurationPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient) : base(navigationService)
        {
            this.apiClient = apiClient;
            this.dialogService = dialogService;
            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            try
            {
                if (Validate())
                {
                    var user = Helper.GetApplicationValue<string>("user");
                    ApiClientOption option = new ApiClientOption
                    {
                        RequestType = ApiClientRequestTypes.Post,
                        Uri = "api/changePassword",
                        BaseUrl = GlobalVariables.BaseUrlApi,
                        RequestContent = new { user, NewPassword }
                    };

                    await apiClient.InvokeDataServiceAsync(option);

                    await dialogService.DisplayAlertAsync("Cambiar contraseña", "Su contraseña se cambio con éxito", "Aceptar");

                    await NavigationService.GoBackAsync();
                }
            }
            catch
            {
                await dialogService.DisplayAlertAsync("Cambiar contraseña", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
            }
        }

        private bool Validate()
        {
            Message = null;

            if (string.IsNullOrWhiteSpace(ActualPassword))
            {
                Message = "Ingresá tu password actual";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
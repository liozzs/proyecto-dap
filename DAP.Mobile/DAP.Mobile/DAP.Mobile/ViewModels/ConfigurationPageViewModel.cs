using DAP.Mobile.Helpers;
using DAP.Mobile.Services;
using EspTouchMultiPlatformLIbrary;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class ConfigurationPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;
        private readonly ISmartConfigTask smartconfig;

        public string WifiPassword { get; set; }
        public string ActualPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public ICommand ConfigWifiCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public ConfigurationPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient)
            : base(navigationService)
        {
            smartconfig = Xamarin.Forms.DependencyService.Get<ISmartConfigHelper>().CreatePlatformTask();
            this.apiClient = apiClient;
            this.dialogService = dialogService;
            ConfigWifiCommand = new DelegateCommand(async () => await ConfigWifiAsync());
            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
            WifiPassword = "lionel13";
        }

        private async Task ConfigWifiAsync()
        {
            if (string.IsNullOrWhiteSpace(WifiPassword))
            {
                await dialogService.DisplayAlertAsync("Error", "Debe ingresar una contraseña", "Aceptar");
            }
            else
            {
                var Ssid = "TeleCentro-8b3c";
                var bssid = "28:9e:fc:0f:8b:41";

                smartconfig.SetSmartConfigTask(Ssid, bssid, WifiPassword, false, 60000);
                IsLoading = true;

                try
                {
                    bool success = false;
                    await Task.Run(() =>
                    {
                        ISmartConfigResult result = smartconfig.executeForResult();
                        if (result.isSuc())
                        {
                            success = true;
                            Helper.SetApplicationValue("ArduinoIP", $"http://{result.getInetAddress()}");
                            Helper.SetApplicationValue("ArduinoMAC", result.getBssid());
                        }
                    });

                    if(success)
                    {
                        await dialogService.DisplayAlertAsync("Configuración WiFi", "Se configuró correctamente.", "Aceptar");
                    }
                }
                catch (Exception ex)
                {
                    await dialogService.DisplayAlertAsync("Configuración WiFi", "No fue posible conectarse.", "Aceptar");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task NextAsync()
        {
            if (Validate())
            {
                try
                {
                    var user = Helper.GetApplicationValue<string>("user");
                    ApiClientOption option = new ApiClientOption
                    {
                        RequestType = ApiClientRequestTypes.Post,
                        Uri = "api/changePassword",
                        Service = ApiClientServices.Api,
                        RequestContent = new { user, NewPassword }
                    };

                    await apiClient.InvokeDataServiceAsync(option);

                    await dialogService.DisplayAlertAsync("Cambiar contraseña", "Su contraseña se cambio con éxito", "Aceptar");

                    await NavigationService.GoBackAsync();
                }
                catch
                {
                    await dialogService.DisplayAlertAsync("Cambiar contraseña", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
                }
            }
        }

        private bool Validate()
        {
            Message = null;

            if (string.IsNullOrWhiteSpace(ActualPassword))
            {
                Message = "Debe ingresar su contraseña actual";
            }
            else if (string.IsNullOrWhiteSpace(NewPassword))
            {
                Message = "Debe ingresar su contraseña nueva";
            }
            else if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                Message = "Confirme la nueva contraseña";
            }
            else if (NewPassword != ConfirmPassword)
            {
                Message = "Las contraseñas no coinciden";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
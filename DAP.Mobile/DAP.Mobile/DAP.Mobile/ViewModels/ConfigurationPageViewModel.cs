using Android.Net.Wifi;
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

        private string bssid;
        private bool hiddenSsid;

        private bool canConfigWifi;
        public bool CanConfigWifi { get => canConfigWifi; set => SetProperty(ref canConfigWifi, value); }

        public string WifiNetwork { get; set; }
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
            ConfigWifiCommand = new DelegateCommand(async () => await ConfigWifiAsync(), () => CanConfigWifi && !IsLoading);
            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
            SetWifiNetwork();
        }

        private void SetWifiNetwork()
        {
            WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            if (wifiManager.IsWifiEnabled && wifiManager.ConnectionInfo.IpAddress != 0)
            {
                WifiNetwork = wifiManager.ConnectionInfo.SSID.Replace("\"", "");
                bssid = wifiManager.ConnectionInfo.BSSID;
                hiddenSsid = wifiManager.ConnectionInfo.HiddenSSID;
                CanConfigWifi = true;
            }
            else
            {
                WifiNetwork = "No se pudo recuperar";
            }
        }

        private async Task ConfigWifiAsync()
        {
            if (string.IsNullOrWhiteSpace(WifiPassword))
            {
                await dialogService.DisplayAlertAsync("Error", "Debe ingresar una contraseña", "Aceptar");
            }
            else
            {
                smartconfig.SetSmartConfigTask(WifiNetwork, bssid, WifiPassword, hiddenSsid, 60000);
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
                            Helper.SetApplicationValue("ArduinoMAC", Helper.FormatBssId(result.getBssid()));
                        }
                    });

                    if (success)
                    {
                        try
                        {
                            ApiClientOption option = new ApiClientOption
                            {
                                RequestType = ApiClientRequestTypes.Post,
                                Uri = "api/usuarios/dispensers",
                                Service = ApiClientServices.Api,
                                RequestContent = new { DireccionMAC = Helper.GetApplicationValue<string>("ArduinoMAC"), Nombre = Helper.GetApplicationValue<string>("ArduinoMAC") }
                            };

                            await apiClient.InvokeDataServiceAsync(option);
                        }
                        catch (Exception ex )
                        {
                            //handle exception
                        }

                        await dialogService.DisplayAlertAsync("Configuración WiFi", "Se configuró correctamente.", "Aceptar");
                    }
                    else
                    {
                        await dialogService.DisplayAlertAsync("Configuración WiFi", "No fue posible conectarse.", "Aceptar");
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
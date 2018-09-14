using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class LoadPillsPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public string PillName { get; set; }
        public int? Quantity { get; set; }
        public int Container { get; set; }

        public IList<int> Containers { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public LoadPillsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient) : base(navigationService)
        {
            this.apiClient = apiClient;
            this.dialogService = dialogService;
            Containers = DataProvider.Containers;
            Container = Containers[0];

            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await AcceptAsync());
        }

        private async Task AcceptAsync()
        {
            try
            {
                if (Validate())
                {
                    bool response = await dialogService.DisplayAlertAsync("Cargar pastillas", $"Ingrese las pastillas en el contenedor {Container} del dispositivo y confirme", "Aceptar", "Cancelar");
                    if (response)
                    {
                        ApiClientOption option = new ApiClientOption
                        {
                            RequestType = ApiClientRequestTypes.Post,
                            Uri = "loadPills",
                            BaseUrl = GlobalVariables.BaseUrlArduino,
                            RequestContent = new { PillName, Container, Quantity }
                        };

                        await apiClient.InvokeDataServiceAsync(option);

                        await dialogService.DisplayAlertAsync("Cargar pastillas", "Se cargaron las pastillas con éxito", "Aceptar");

                        await NavigationService.GoBackAsync();
                    }
                }
            }
            catch
            {
                await dialogService.DisplayAlertAsync("Cargar pastillas", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
            }

        }

        private bool Validate()
        {
            Message = null;

            if (!string.IsNullOrWhiteSpace(PillName))
            {
                Message = "Debe ingresar el nombre de la pastilla";
            }
            else if (!Quantity.HasValue)
            {
                Message = "Debe ingresar la cantidad de pastillas";
            }
            else if (Quantity.GetValueOrDefault() <= 0)
            {
                Message = "Debe ingresar una cantidad mayor a 0";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
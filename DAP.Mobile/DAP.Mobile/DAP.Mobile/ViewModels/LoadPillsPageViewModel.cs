using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
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
        public string Quantity { get; set; }
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
            Quantity = "20";

            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await AcceptAsync());
        }

        private async Task AcceptAsync()
        {

            if (Validate())
            {
                bool response = await dialogService.DisplayAlertAsync("Cargar pastillas", $"Ingrese las pastillas en el contenedor {Container} del dispositivo y confirme", "Aceptar", "Cancelar");
                if (response)
                {
                    try
                    {
                        var qty = Convert.ToInt32(Quantity);
                        ApiClientOption option = new ApiClientOption
                        {
                            RequestType = ApiClientRequestTypes.Post,
                            Uri = "loadPills",
                            Service = ApiClientServices.Arduino,
                            RequestContent = new { PillName, Container, qty }
                        };

                        await apiClient.InvokeDataServiceAsync(option);

                        await dialogService.DisplayAlertAsync("Cargar pastillas", "Se cargaron las pastillas con éxito", "Aceptar");

                        await NavigationService.GoBackAsync();
                    }
                    catch
                    {
                        await dialogService.DisplayAlertAsync("Cargar pastillas", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
                    }
                }
            }
        }

        private bool Validate()
        {
            Message = null;

            if (string.IsNullOrWhiteSpace(PillName))
            {
                Message = "Debe ingresar el nombre de la pastilla";
            }
            else if (!Int32.TryParse(Quantity, out int qty))
            {
                Message = "Debe ingresar la cantidad de pastillas";
            }
            else if (qty <= 0)
            {
                Message = "Debe ingresar una cantidad mayor a 0";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
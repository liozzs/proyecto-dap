using DAP.Mobile.Models;
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
        private readonly ISqliteService sqliteService;
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public string PillName { get; set; }
        public string Quantity { get; set; }
        public int Container { get; set; }

        public IList<int> Containers { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public LoadPillsPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient, ISqliteService sqliteService) : base(navigationService)
        {
            this.sqliteService = sqliteService;
            this.apiClient = apiClient;
            this.dialogService = dialogService;
            Containers = new List<int> { 1, 2 };
            Container = Containers[0];
            Quantity = "20";

            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await AcceptAsync());
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            var pill = parameters.GetValue<Pill>("Pill");
            if (pill != null)
            {
                Container = pill.Container;
                PillName = pill.Name;
                Quantity = pill.Quantity.ToString();
            }
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

                        Pill pill = new Pill { Name = PillName, Container = Container, Quantity = qty };

                        await sqliteService.Save(pill);

                        ApiClientOption option = new ApiClientOption
                        {
                            RequestType = ApiClientRequestTypes.Post,
                            Uri = "loadPills",
                            Service = ApiClientServices.Arduino,
                            RequestContent = pill
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
using DAP.Mobile.Helpers;
using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class NotificationsPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService pageDialogService;
        private readonly IApiClient apiClient;

        public DelegateCommand LoadNotificationsCommand { get; set; }

        private IList<Notification> notifications;
        public IList<Notification> Notifications
        {
            get => notifications;
            set => SetProperty(ref notifications, value);
        }

        public NotificationsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, IApiClient apiClient) : base(navigationService)
        {
            this.pageDialogService = pageDialogService;
            this.apiClient = apiClient;
            Notifications = new ObservableCollection<Notification>();
            LoadNotificationsCommand = new DelegateCommand(async () => await LoadNotificationsAsync());
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            LoadNotificationsCommand.Execute();
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                IsLoading = true;

                ApiClientOption option = new ApiClientOption
                {
                    RequestType = ApiClientRequestTypes.Get,
                    UriParameters = new { MAC = Helper.GetApplicationValue<string>("ArduinoMAC") },
                    Uri = "api/usuarios/dispensers/{MAC}/mensajes",
                    Service = ApiClientServices.Api
                };

                IList<Mensaje> mensajes = await apiClient.InvokeDataServiceAsync<IList<Mensaje>>(option);

                Notifications = new ObservableCollection<Notification>(mensajes.OrderByDescending(m => m.Id).Select(m => new Notification { Id = m.Id, Message = GetMessage(m), Title = m.Pastilla, Date = m.Horario }));
            }
            catch
            {
                await pageDialogService.DisplayAlertAsync("Error", "Ocurrió un error al obtener las notificaciones. Intente nuevamente en unos minutos.", "Aceptar");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetMessage(Mensaje mensaje)
        {
            string message = "";

            var parameters = new Dictionary<string, string>
            {
                { "Receptáculo", mensaje.Receptaculo.ToString() },
                { "Pastilla", mensaje.Pastilla }
            };
            
            switch (mensaje.Codigo)
            {
                case CodigoError.FALTA_DE_PASTILLAS:
                    message = "Expendio no realizado. Cantidad de pastillas en el receptáculo menor a la cantidad de pastillas a dispensar.";
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Cantidad de pastillas en el receptáculo menor a la cantidad de pastillas a dispensar.");
                    break;
                case CodigoError.LIMITE_DE_TIEMPO:
                    message = "Expendio no realizado. Se superó el umbral de tiempo en que el mecanismo debe realizar el expendio.";
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Se superó el umbral de tiempo en que el mecanismo debe realizar el expendio.");
                    break;
                case CodigoError.STOCK_CRITICO:
                    message = "Expendio realizado. Se ha alcanzado el stock crítico.";
                    message += "Le informamos se ha alcanzado el stock crítico del siguiente medicamento:\n";
                    parameters.Add("Cantidad Restante", mensaje.CantidadRestante.ToString());
                    break;
                case CodigoError.BOTON_NO_PRESIONADO:
                    message = "Expendio no realizado. El botón para iniciar el expendio de medicamentos no ha sido presionado.";
                    message += "Le informamos que el botón para iniciar el expendio de medicamentos no ha sido presionado. Expendio correspondiente:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.VASO_NO_RETIRADO:
                    message = "Expendio realizado. El recipiente no ha sido retirado de su posición.";
                    message += "Le informamos que el recipiente no ha sido retirado de su posición. Último expendio realizado:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.VASO_NO_DEVUELTO:
                    message = "Expendio realizado. El recipiente no ha sido devuelto a su posición.";
                    message += "Le informamos que el recipiente no ha sido devuelto a su posición. Último expendio realizado:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case CodigoError.BLOQUEO_RECIPIENTE:
                    message = "Le informamos se ha realizado el bloqueo del receptáculo.";
                    message += "Le informamos se ha realizado el bloqueo del siguiente receptáculo. Expendio correspondiente:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    break;
            }

            foreach (KeyValuePair<string, string> param in parameters)
            {
                message += $"{param.Key}: {param.Value}\n";
            }

            return message;
        }
    }

    public enum CodigoError
    {
        FALTA_DE_PASTILLAS = 1,
        LIMITE_DE_TIEMPO = 2,
        STOCK_CRITICO = 3,
        BOTON_NO_PRESIONADO = 4,
        VASO_NO_RETIRADO = 5,
        VASO_NO_DEVUELTO = 6,
        BLOQUEO_RECIPIENTE = 7
    }

    public class Mensaje
    {
        public int Id { get; set; }
        public int DispenserID { get; set; }
        public CodigoError Codigo { get; set; }
        public int Receptaculo { get; set; }
        public string Pastilla { get; set; }
        public string Horario { get; set; }
        public int CantidadRestante { get; set; }
        public string DireccionMAC { get; set; }
    }
}
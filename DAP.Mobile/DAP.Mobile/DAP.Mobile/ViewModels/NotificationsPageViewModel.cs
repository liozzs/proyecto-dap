using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
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
                    Uri = "api/usuarios/dispensers",
                    Service = ApiClientServices.Api
                };

                IList<Dispenser> dispensers = await apiClient.InvokeDataServiceAsync<IList<Dispenser>>(option);
                var mensajes = dispensers.SelectMany(d => d.Mensajes)
                    .OrderByDescending(m => m.Id);

                Notifications = new ObservableCollection<Notification>(mensajes.Select(m => new Notification { Id = m.Id, Message = GetMessage(m), Title = m.Pastilla, Date = m.Horario }));
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
                case 1:
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Cantidad de pastillas en el receptáculo menor a la cantidad de pastillas a dispensar.");
                    break;
                case 2:
                    message += "Le informamos no se ha podido realizar el expendio del siguiente medicamento:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    parameters.Add("Causa", "Se superó el umbral de tiempo en que el mecanismo debe realizar el expendio.");
                    break;
                case 3:
                    message += "Le informamos se ha alcanzado el stock crítico del siguiente medicamento:\n";
                    parameters.Add("Cantidad Restante", mensaje.CantidadRestante.ToString());
                    break;
                case 4:
                    message += "Le informamos que el botón para iniciar el expendio de medicamentos no ha sido presionado. Expendio correspondiente:\n";
                    parameters.Add("Horario", mensaje.Horario);
                    break;
                case 5:
                    message += "Le informamos que el recipiente no ha sido devuelto a su posición. Último expendio realizado:\n";
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
    
    public class Mensaje
    {
        public int Id { get; set; }
        public int DispenserID { get; set; }
        public int Codigo { get; set; }
        public int Receptaculo { get; set; }
        public string Pastilla { get; set; }
        public string Horario { get; set; }
        public int CantidadRestante { get; set; }
        public string DireccionMAC { get; set; }
    }

    public class Dispenser
    {
        public int Id { get; set; }
        public string DireccionMAC { get; set; }
        public string Nombre { get; set; }
        public IList<Mensaje> Mensajes { get; set; }
    }
}
using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationActionPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public IList<PlanificationAction> Actions { get; set; }

        private PlanificationAction action;

        public PlanificationAction Action
        {
            get
            {
                return action;
            }
            set
            {
                SetProperty(ref action, value);
            }
        }

        public PlanificationActionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient) : base(navigationService)
        {
            this.apiClient = apiClient;
            this.dialogService = dialogService;

            Actions = new List<PlanificationAction>
            {
                new PlanificationAction() { Id = 1, Name = "Ninguna", Description= "La planificación seguirá dispensando la medicación en los horarios establecidos." },
                new PlanificationAction() { Id = 2, Name = "Replanificar", Description= "En caso de no haber tomado la medicación en el momento indicado, se replanificarán los próximos expendios, corriendo el horario para cumplir con los intervalos establecidos." },
                new PlanificationAction() { Id = 3, Name = "Bloquear", Description= "Al pasar una hora sin haber tomado la medicación, la planificación se bloqueará y no se dispensarán más medicamentos, dando por finalizado el tratamiento." }
            };
            Action = Actions[0];

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            PlanificationBuilder.SetAction(Action);

            Planification planification = PlanificationBuilder.Build();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Pastilla: {planification.Pill.Name}");
            sb.AppendLine($"Inicio: {planification.StartDate:dd/MM/yyyy} {planification.StartTime:HH:mm}");
            if(planification.Interval>0)
            {
                sb.AppendLine($"Intervalo: Cada {planification.Interval} hs.");
            }
            if(planification.Days != null)
            {
                var days = new List<string> { "Lu", "Ma", "Mi", "Ju", "Vi", "Sá", "Do" };
                string d = "";
                for (int i = 0; i < planification.Days.Count; i++)
                {
                    if (planification.Days[i])
                    {
                        d += days[i] + " ";
                    }
                }
                sb.AppendLine($"Días: {d}");
            }
            else
            {
                sb.AppendLine($"Días: Todos");
            }
            sb.AppendLine($"Cantidad a dispensar: {planification.QtyToDispense}");
            sb.AppendLine($"Stock crítico: {planification.CriticalStock}");
            sb.AppendLine($"Acción: {planification.Action.Description}");
            var response = await dialogService.DisplayAlertAsync("Confirmar", sb.ToString(), "Sí", "No");
            if (response)
            {
                try
                {
                    ApiClientOption option = new ApiClientOption
                    {
                        RequestType = ApiClientRequestTypes.Post,
                        Uri = "plain",
                        Service = ApiClientServices.Arduino,
                        RequestContent = planification
                    };

                    await apiClient.InvokeDataServiceAsync(option);

                    await dialogService.DisplayAlertAsync("Planificación", "Se creó la planificación con éxito", "Aceptar");

                    await NavigationService.GoBackToRootAsync();
                }
                catch
                {
                    await dialogService.DisplayAlertAsync("Planificación", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
                }

            }
        }
    }
}
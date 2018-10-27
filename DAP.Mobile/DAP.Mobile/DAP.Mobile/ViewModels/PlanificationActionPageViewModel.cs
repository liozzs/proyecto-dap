using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationActionPageViewModel : ViewModelBase
    {
        private readonly ISqliteService sqliteService;
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public IList<PlanificationAction> Actions { get; set; }

        private Planification planification;

        private PlanificationAction action;
        public PlanificationAction Action
        {
            get => action;
            set => SetProperty(ref action, value);
        }

        public PlanificationActionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient, ISqliteService sqliteService) : base(navigationService)
        {
            this.sqliteService = sqliteService;
            this.apiClient = apiClient;
            this.dialogService = dialogService;

            Actions = sqliteService.Get<PlanificationAction>().Result;
            Action = Actions[0];

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            PlanificationBuilder.SetAction(Action);

            Planification planif = PlanificationBuilder.Build();

            var response = await ShowConfirmMessage(planif);
            if (response)
            {
                try
                {
                    //ApiClientOption option = new ApiClientOption
                    //{
                    //    RequestType = ApiClientRequestTypes.Post,
                    //    Uri = "plain",
                    //    Service = ApiClientServices.Arduino,
                    //    RequestContent = new
                    //    {
                    //        StartTime = $"{planif.StartDate}{planif.StartTime}",
                    //        Interval = planif.Interval * 60 * 60,
                    //        Quantity = planif.QtyToDispense,
                    //        planif.CriticalStock,
                    //        Periodicity = Convert.ToInt32(planif.Type),
                    //        planif.Days,
                    //        Block = planif.ActionId,
                    //        PlateId = planif.Container * 100
                    //    }
                    //};

                    //await apiClient.InvokeDataServiceAsync(option);

                    await sqliteService.Save(planif);

                    await dialogService.DisplayAlertAsync("Planificación", "Se creó la planificación con éxito", "Aceptar");

                    await NavigationService.GoBackToRootAsync();
                }
                catch
                {
                    await dialogService.DisplayAlertAsync("Planificación", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
                }
            }
        }

        private Task<bool> ShowConfirmMessage(Planification planif)
        {
            StringBuilder sb = new StringBuilder();

            var startDate = DateTime.ParseExact(planif.StartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            var startTime = DateTime.ParseExact(planif.StartTime, "HHmmss", CultureInfo.InvariantCulture);

            sb.AppendLine($"Pastilla: {planif.PillName}");
            sb.AppendLine($"Inicio: {startDate:dd/MM/yyyy} {startTime:HH:mm}");
            if (planif.Interval > 0)
            {
                sb.AppendLine($"Intervalo: Cada {planif.Interval} hs.");
            }
            if (!string.IsNullOrEmpty(planif.Days.TrimStart('0')))
            {
                var days = new List<string> { "Lu", "Ma", "Mi", "Ju", "Vi", "Sá", "Do" };
                string d = "";
                for (int i = 0; i < planif.Days.Length; i++)
                {
                    if (planif.Days[i] == '1')
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
            sb.AppendLine($"Cantidad a dispensar: {planif.QtyToDispense}");
            sb.AppendLine($"Stock crítico: {planif.CriticalStock}");
            sb.AppendLine($"Acción: {planif.ActionDescription}");
            return dialogService.DisplayAlertAsync("Confirmar", sb.ToString(), "Sí", "No");
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            planification = parameters.GetValue<Planification>("Planification");
            if (planification != null)
            {
                Action = Actions.SingleOrDefault(action => action.Id == planification.ActionId) ?? Actions[0];
            }
        }
    }
}
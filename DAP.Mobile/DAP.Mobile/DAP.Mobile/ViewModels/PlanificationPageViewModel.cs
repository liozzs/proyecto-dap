using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationPageViewModel : ViewModelBase
    {
        private Planification planification;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public IList<Pill> Pills { get; set; }
        public IList<Periodicity> Periodicities { get; set; }

        private Pill pill;
        public Pill Pill { get => pill; set => SetProperty(ref pill, value); }

        private Periodicity periodicity;
        public Periodicity Periodicity { get => periodicity; set => SetProperty(ref periodicity, value); }

        private string criticalStock;
        public string CriticalStock { get => criticalStock; set => SetProperty(ref criticalStock, value); }

        private string qtyToDispense;
        public string QtyToDispense { get => qtyToDispense; set => SetProperty(ref qtyToDispense, value); }

        private DateTime startDate;
        public DateTime StartDate { get => startDate; set => SetProperty(ref startDate, value); }

        public PlanificationPageViewModel(INavigationService navigationService, ISqliteService sqliteService) : base(navigationService)
        {
            Pills = sqliteService.Get<Pill>().Result;
            Pill = Pills[0];

            StartDate = DateTime.Today;

            Periodicities = new List<Periodicity>()
            {
                new Periodicity { Id = (int)PlanificationType.Daily, Description = "Diaria", NextPage = "DailyPlanificationPage" },
                new Periodicity { Id = (int)PlanificationType.Weekly, Description = "Semanal", NextPage = "WeeklyPlanificationPage" },
                new Periodicity { Id = (int)PlanificationType.Custom, Description = "Personalizada", NextPage = "CustomPlanificationPage" }
            };
            Periodicity = Periodicities[0];
            CriticalStock = "0";
            QtyToDispense = "1";

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            planification = parameters.GetValue<Planification>("Planification");
            if (planification != null)
            {
                Pill = Pills.SingleOrDefault(p => p.Id == planification.PillId) ?? Pills[0];
                Periodicity = Periodicities.SingleOrDefault(p => p.Id == planification.Type) ?? Periodicities[0];
                CriticalStock = planification.CriticalStock.ToString();
                QtyToDispense = planification.QtyToDispense.ToString();
                StartDate = DateTime.ParseExact(planification.StartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                PlanificationBuilder.SetId(planification.Id);
            }
        }

        private async Task NextAsync()
        {
            if (Validate())
            {
                PlanificationBuilder.SetPlanificationType((PlanificationType)Periodicity.Id);
                PlanificationBuilder.SetPill(Pill);
                PlanificationBuilder.SetStartDate(StartDate);
                PlanificationBuilder.SetCriticalStock(string.IsNullOrEmpty(CriticalStock) ? 0 : Convert.ToInt32(CriticalStock));
                PlanificationBuilder.SetQtyToDispense(Convert.ToInt32(QtyToDispense));

                await NavigationService.NavigateAsync(Periodicity.NextPage, new NavigationParameters() { { "Planification", planification } });
            }
        }

        private bool Validate()
        {
            Message = null;
            if (Pill == null)
            {
                Message = "Seleccione una pastilla";
            }
            else if (string.IsNullOrEmpty(QtyToDispense))
            {
                Message = "Debe ingresar la cantidad a dispensar";
            }
            else if (!Int32.TryParse(QtyToDispense, out int qty))
            {
                Message = "La cantidad a dispensar ingresada es inválida";
            }
            else if (qty <= 0)
            {
                Message = "La cantidad a dispensar debe ser mayor a 0";
            }
            else if (!String.IsNullOrEmpty(CriticalStock))
            {
                if (!Int32.TryParse(CriticalStock, out int criticalStk))
                {
                    Message = "El stock crítico ingresado es inválido";
                }
                else if (criticalStk < 0)
                {
                    Message = "El stock crítico debe ser mayor o igual a 0";
                }
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationPageViewModel : ViewModelBase
    {
        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public IList<Pill> Pills { get; set; }
        public IList<Periodicity> Periodicities { get; set; }

        public Pill Pill { get; set; }
        public Periodicity Periodicity { get; set; }
        public string CriticalStock { get; set; }
        public string QtyToDispense { get; set; }

        public DateTime StartDate { get; set; }

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

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            var planif = parameters.GetValue<Planification>("Planification");
            if (planif != null)
            {
                
            }
        }

        private async Task NextAsync()
        {
            if (Validate())
            {
                PlanificationBuilder.SetType((PlanificationType)Periodicity.Id);
                PlanificationBuilder.SetPill(Pill);
                PlanificationBuilder.SetStartDate(StartDate);
                PlanificationBuilder.SetCriticalStock(string.IsNullOrEmpty(CriticalStock) ? 0 : Convert.ToInt32(CriticalStock));
                PlanificationBuilder.SetQtyToDispense(Convert.ToInt32(QtyToDispense));

                await NavigationService.NavigateAsync(Periodicity.NextPage);
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
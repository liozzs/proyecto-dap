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
        public int CriticalStock { get; set; }
        public int QtyToDispense { get; set; }
        public DateTime StartDate { get; set; }

        public PlanificationPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            StartDate = DateTime.Today;
            Pills = DataProvider.Pills;
            Pill = Pills[0];
            Periodicities = DataProvider.Periodicities;
            Periodicity = Periodicities[0];
            CriticalStock = 0;
            QtyToDispense = 1;

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            if (Validate())
            {
                PlanificationBuilder.Create((PlanificationType)Periodicity.Id, Pill, StartDate);

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
            else if (QtyToDispense == 0)
            {
                Message = "Debe ingresar la cantidad a dispensar";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
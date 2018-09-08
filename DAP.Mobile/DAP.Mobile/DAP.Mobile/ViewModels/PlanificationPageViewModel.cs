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
    public class PlanificationPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public IList<Pill> Pills { get; set; }
        public IList<Periodicity> Periodicities { get; set; }

        public Pill Pill { get; set; }
        public Periodicity Periodicity { get; set; }
        public int? CriticalStock { get; set; }
        public int? QtyToDispense { get; set; }
        public DateTime StartDate { get; set; }

        public PlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;
            StartDate = DateTime.Today;
            Pills = DataProvider.Pills;
            Pill = Pills[0];
            Periodicities = DataProvider.Periodicities;
            Periodicity = Periodicities[0];
            CriticalStock = 0;
            QtyToDispense = 1;

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            if (Pill == null)
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione una pastilla", "Aceptar");
            }
            if (Periodicity == null)
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione una periodicidad", "Aceptar");
            }

            return NavigationService.NavigateAsync(Periodicity.NextPage);
        }
    }
}
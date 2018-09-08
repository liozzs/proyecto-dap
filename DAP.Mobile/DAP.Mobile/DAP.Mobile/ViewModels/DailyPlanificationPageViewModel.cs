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
    public class DailyPlanificationPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public DailyInterval DailyPeriodicity { get; set; }
        public DateTime StartTime { get; set; }
        public IList<DailyInterval> DailyIntervals { get; set; }

        public DailyPlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;
            DailyIntervals = DataProvider.DailyIntervals;
            DailyPeriodicity = DailyIntervals[0];

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            if (DailyPeriodicity == null)
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione una periodicidad", "Aceptar");
            }

            return NavigationService.NavigateAsync("PlanificationActionPage");
        }
    }
}
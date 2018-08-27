using DAP.Mobile.Models;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class DailyPlanificationPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public Periodicity SelectedDailyPeriodicity { get; set; }
        public DateTime StartTime { get; set; }

        public DailyPlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            if (SelectedDailyPeriodicity == null)
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione una periodicidad", "Aceptar");
            }

            return NavigationService.NavigateAsync("PlanificationActionPage");
        }
    }
}

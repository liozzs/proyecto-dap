using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class WeeklyPlanificationPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;
        private WeeklyInterval weekInterval;

        public WeeklyInterval WeekInterval
        {
            get { return weekInterval; }
            set
            {
                weekInterval = value;
                if (value != null)
                {
                    weekInterval.IsSelected = !weekInterval.IsSelected;
                }
            }
        }

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public IList<WeeklyInterval> WeeklyIntervals { get; private set; }

        public WeeklyPlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;
            WeeklyIntervals = DataProvider.WeeklyIntervals;

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            if (WeeklyIntervals.Any(i => !i.IsSelected))
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione al menos una periodicidad", "Aceptar");
            }

            return NavigationService.NavigateAsync("PlanificationActionPage");
        }
    }
}
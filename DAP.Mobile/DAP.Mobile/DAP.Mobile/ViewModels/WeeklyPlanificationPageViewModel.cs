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
        public IList<bool> Days { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public WeeklyPlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            Days = new List<bool> { false, false, false, false, false, false, false };
            this.dialogService = dialogService;

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            if (Days.All(i => !i))
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione al menos una periodicidad", "Aceptar");
            }

            return NavigationService.NavigateAsync("PlanificationActionPage");
        }
    }
}
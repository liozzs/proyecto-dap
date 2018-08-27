using DAP.Mobile.Models;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public Pill SelectedPill { get; set; }
        public Periodicity SelectedPeriodicity { get; set; }
        public int? CriticalStock { get; set; }
        public int? QtyToDispense { get; set; }
        public DateTime StartDate { get; set; }

        public PlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;
            StartDate = DateTime.Today;

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            if(SelectedPeriodicity == null)
            {
                return dialogService.DisplayAlertAsync("Validación", "Seleccione una periodicidad", "Aceptar");
            }

            return NavigationService.NavigateAsync(SelectedPeriodicity.NextPage);
        }
    }
}

using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationActionPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public IList<PlanificationAction> Actions { get; set; }

        private PlanificationAction action;
        public PlanificationAction Action
        {
            get
            {
                return action;
            }
            set
            {
                SetProperty(ref action, value);
            }
        }

        public PlanificationActionPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService)
        {
            this.dialogService = dialogService;

            Actions = DataProvider.PlanificationActions;
            Action = Actions[0];

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            if (Action == null)
            {
                await dialogService.DisplayAlertAsync("Validación", "Seleccione una acción", "Aceptar");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Acá iría toda la explicación de la planificacion....");
            var response = await dialogService.DisplayAlertAsync("Confirmar", sb.ToString(), "Sí", "No");
            if (response)
            {
                await dialogService.DisplayAlertAsync("Felicidades", "Creaste una planificacion!", "Aceptar");
                await NavigationService.GoBackToRootAsync();
            }
        }
    }
}
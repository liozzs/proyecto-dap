using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class PlanificationListPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService pageDialogService;
        private readonly ISqliteService sqliteService;

        public DelegateCommand<Planification> OpenPlanificationCommand { get; set; }
        public DelegateCommand CreateCommand { get; set; }

        private IList<Planification> planifications;
        public IList<Planification> Planifications
        {
            get => planifications;
            set => SetProperty(ref planifications, value);
        }

        public PlanificationListPageViewModel(INavigationService navigationService, ISqliteService sqliteService, IPageDialogService pageDialogService) : base(navigationService)
        {
            this.pageDialogService = pageDialogService;
            this.sqliteService = sqliteService;
            OpenPlanificationCommand = new DelegateCommand<Planification>(async planif => await OpenPlanificationAsync(planif));
            CreateCommand = new DelegateCommand(async () => await OpenPlanificationAsync());
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            this.Planifications = sqliteService.Get<Planification>().Result;
        }

        private async Task OpenPlanificationAsync(Planification planif = null)
        {
            var pills = await sqliteService.Get<Pill>();
            if (pills.Any())
            {
                await NavigationService.NavigateAsync("PlanificationPage", new NavigationParameters() { { "Planification", planif } });
            }
            else
            {
                await pageDialogService.DisplayAlertAsync("Error", "Debe cargar pastillas antes de crear una planificación.", "Aceptar");
            }
        }
    }
}

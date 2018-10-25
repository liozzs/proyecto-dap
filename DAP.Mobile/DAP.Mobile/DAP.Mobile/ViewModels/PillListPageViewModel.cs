using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class PillListPageViewModel : ViewModelBase
	{
        private readonly ISqliteService sqliteService;

        public DelegateCommand<Pill> OpenPillCommand { get; set; }
        public DelegateCommand LoadPillsCommand { get; set; }
        public DelegateCommand CreateCommand { get; set; }
        public ObservableCollection<Pill> Pills { get; set; }

        public PillListPageViewModel(INavigationService navigationService, ISqliteService sqliteService) : base(navigationService)
        {
            this.sqliteService = sqliteService;
            OpenPillCommand = new DelegateCommand<Pill>(async pill => await OpenPillAsync(pill));
            CreateCommand = new DelegateCommand(async () => await OpenPillAsync());
            LoadPillsCommand = new DelegateCommand(async () => await LoadPillsAsync());
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            LoadPillsCommand.Execute();
        }

        private async Task LoadPillsAsync()
        {
            IsLoading = true;
            Pills = new ObservableCollection<Pill>(await sqliteService.Get<Pill>());
            IsLoading = false;
        }

        private async Task OpenPillAsync(Pill pill = null)
        {
            await NavigationService.NavigateAsync("LoadPillsPage", new NavigationParameters() { { "Pill", pill } });
        }
    }
}

using Prism.Navigation;
using Prism.Services;

namespace DAP.Mobile.ViewModels
{
    public class CustomPlanificationPageViewModel : WeeklyPlanificationPageViewModel
    {
        public int Interval { get; set; }
        public CustomPlanificationPageViewModel(INavigationService navigationService, IPageDialogService dialogService) : base(navigationService, dialogService)
        {
            Interval = 8;
        }
    }
}
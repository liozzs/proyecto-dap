using Prism.Navigation;

namespace DAP.Mobile.ViewModels
{
    public class CustomPlanificationPageViewModel : WeeklyPlanificationPageViewModel
    {
        public CustomPlanificationPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Interval = 8;
        }

        protected override bool Validate()
        {
            if(Interval.GetValueOrDefault() > 24)
            {
                Message = "El intervalo debe ser menor a 24 hs";
            }

            return base.Validate();
        }
    }
}
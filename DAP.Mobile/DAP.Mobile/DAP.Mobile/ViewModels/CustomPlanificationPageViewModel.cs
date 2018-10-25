using Prism.Navigation;

namespace DAP.Mobile.ViewModels
{
    public class CustomPlanificationPageViewModel : WeeklyPlanificationPageViewModel
    {
        public CustomPlanificationPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Interval = "8";
        }

        protected override bool Validate()
        {

            if (!string.IsNullOrEmpty(Interval))
            {
                if (!int.TryParse(Interval, out int interval))
                {
                    Message = "El intervalo ingresado es inválido";
                }
                else if (interval < 6 || interval > 24)
                {
                    Message = "El intervalo debe estar entre 6 y 24 hs";
                }
            }

            return base.Validate();
        }
    }
}
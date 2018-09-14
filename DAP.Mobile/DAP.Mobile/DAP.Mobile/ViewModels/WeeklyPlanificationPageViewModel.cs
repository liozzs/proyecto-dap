using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class WeeklyPlanificationPageViewModel : ViewModelBase
    {
        public IList<bool> Days { get; set; }
        public DateTime StartTime { get; set; }
        public int? Interval { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public WeeklyPlanificationPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Days = new List<bool> { false, false, false, false, false, false, false };

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await NextAsync());
        }

        private async Task NextAsync()
        {
            Message = null;

            if (Validate())
            {
                PlanificationBuilder.SetInterval(StartTime, Interval, Days);
                await NavigationService.NavigateAsync("PlanificationActionPage");
            }
        }

        protected virtual bool Validate()
        {
            if (Days.All(i => !i))
            {
                Message = "Seleccione al menos un día";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}
using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class DailyPlanificationPageViewModel : ViewModelBase
    {
        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public DailyInterval DailyPeriodicity { get; set; }
        public TimeSpan StartTime { get; set; }
        public IList<DailyInterval> DailyIntervals { get; set; }

        public DailyPlanificationPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            DailyIntervals = new List<DailyInterval>()
            {
                new DailyInterval { Id = 6, Description = "Cada 6 hs" },
                new DailyInterval { Id = 8, Description = "Cada 8 hs" },
                new DailyInterval { Id = 12, Description = "Cada 12 hs" },
                new DailyInterval { Id = 24, Description = "Cada 24 hs" }
            };
            DailyPeriodicity = DailyIntervals[0];

            CancelCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
            NextCommand = new DelegateCommand(async () => await Next());
        }

        private Task Next()
        {
            PlanificationBuilder.SetInterval(StartTime, DailyPeriodicity.Id, null);
            return NavigationService.NavigateAsync("PlanificationActionPage");
        }
    }
}
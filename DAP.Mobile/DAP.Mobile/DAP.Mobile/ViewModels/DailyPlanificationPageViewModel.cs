using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class DailyPlanificationPageViewModel : ViewModelBase
    {
        private Planification planification;

        public ICommand CancelCommand { get; set; }
        public ICommand NextCommand { get; set; }

        private DailyInterval dailyPeriodicity;
        public DailyInterval DailyPeriodicity { get => dailyPeriodicity; set => SetProperty(ref dailyPeriodicity, value); }

        private TimeSpan startTime;
        public TimeSpan StartTime { get => startTime; set => SetProperty(ref startTime, value); }

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
            return NavigationService.NavigateAsync("PlanificationActionPage", new NavigationParameters() { { "Planification", planification } });
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            planification = parameters.GetValue<Planification>("Planification");
            if (planification != null)
            {
                DailyPeriodicity = DailyIntervals.SingleOrDefault(dp => dp.Id == planification.Interval) ?? DailyIntervals[0];
                DateTime d = DateTime.ParseExact(planification.StartTime, "HHmmss", CultureInfo.InvariantCulture);
                StartTime = new TimeSpan(d.Hour, d.Minute, 0);
            }
        }
    }
}
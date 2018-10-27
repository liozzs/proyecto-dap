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
    public class WeeklyPlanificationPageViewModel : ViewModelBase
    {
        private Planification planification;

        private IList<bool> days;
        public IList<bool> Days { get => days; set => SetProperty(ref days, value); }

        private TimeSpan startTime;
        public TimeSpan StartTime { get => startTime; set => SetProperty(ref startTime, value); }

        private string interval;
        public string Interval { get => interval; set => SetProperty(ref interval, value); }

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
                PlanificationBuilder.SetInterval(StartTime, string.IsNullOrEmpty(Interval) ? 0 : Convert.ToInt32(Interval), Days);
                await NavigationService.NavigateAsync("PlanificationActionPage", new NavigationParameters() { { "Planification", planification } });
            }
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            planification = parameters.GetValue<Planification>("Planification");
            if (planification != null)
            {
                Days = planification.Days.Select(c => c == '1').ToList();
                Interval = planification.Interval.ToString();
                StartTime = TimeSpan.ParseExact(planification.StartTime, "HHmmss", CultureInfo.InvariantCulture);
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
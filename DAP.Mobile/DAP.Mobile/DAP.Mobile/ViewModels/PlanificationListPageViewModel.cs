﻿using DAP.Mobile.Models;
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

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            var sqlPlanifications = sqliteService.Get<Planification>().Result;
            var pills = sqliteService.Get<Pill>().Result;
            sqlPlanifications.ForEach(p => p.PillName = pills.SingleOrDefault(pill => pill.Id == p.PillId)?.Name);
            this.Planifications = sqlPlanifications;
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
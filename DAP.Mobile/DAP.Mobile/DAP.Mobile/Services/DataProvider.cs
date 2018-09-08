using DAP.Mobile.Models;
using System.Collections.Generic;

namespace DAP.Mobile.Services
{
    public static class DataProvider
    {
        public static IList<DailyInterval> DailyIntervals { get; private set; }
        public static IList<Periodicity> Periodicities { get; private set; }
        public static IList<Pill> Pills { get; private set; }
        public static IList<int> Containers { get; set; }
        public static IList<PlanificationAction> PlanificationActions { get; internal set; }

        static DataProvider()
        {
            FillPeriodicities();
            FillDailyPeriodicities();
            FillPills();
            FillContainers();
            FillActions();
        }

        private static void FillActions()
        {
            PlanificationActions = new List<PlanificationAction>
            {
                new PlanificationAction() { Id = 1, Name = "Ninguna", Description= "La planificación seguirá dispensando la medicación en los horarios establecidos." },
                new PlanificationAction() { Id = 2, Name = "Replanificar", Description= "En caso de no haber tomado la medicación en el momento indicado, se replanificarán los próximos expendios, corriendo el horario para cumplir con los intervalos establecidos." },
                new PlanificationAction() { Id = 3, Name = "Bloquear", Description= "Al pasar una hora sin haber tomado la medicación, la planificación se bloqueará y no se dispensarán más medicamentos, dando por finalizado el tratamiento." }
            };
        }

        private static void FillContainers()
        {
            Containers = new List<int> { 1, 2 };
        }

        private static void FillPills()
        {
            Pills = new List<Pill>();
            for (int i = 1; i <= 5; i++)
            {
                Pills.Add(new Pill() { Id = i, Name = $"Pastilla {i}" });
            }
        }

        private static void FillPeriodicities()
        {
            Periodicities = new List<Periodicity>()
            {
                new Periodicity { Id = 1, Description = "Diaria", NextPage = "DailyPlanificationPage" },
                new Periodicity { Id = 2, Description = "Semanal", NextPage = "WeeklyPlanificationPage" },
                new Periodicity { Id = 3, Description = "Personalizada", NextPage = "CustomPlanificationPage" }
            };
        }

        private static void FillDailyPeriodicities()
        {
            DailyIntervals = new List<DailyInterval>()
            {
                new DailyInterval { Id = 1, Description = "Cada 6 hs" },
                new DailyInterval { Id = 2, Description = "Cada 8 hs" },
                new DailyInterval { Id = 3, Description = "Cada 12 hs" },
                new DailyInterval { Id = 4, Description = "Cada 24 hs" }
            };
        }
    }
}
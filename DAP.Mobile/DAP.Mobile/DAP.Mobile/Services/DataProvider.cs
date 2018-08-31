using DAP.Mobile.Models;
using System.Collections.Generic;

namespace DAP.Mobile.Services
{
    public static class DataProvider
    {
        public static IList<WeeklyInterval> WeeklyIntervals { get; private set; }
        public static IList<DailyInterval> DailyIntervals { get; private set; }
        public static IList<Periodicity> Periodicities { get; private set; }
        public static IList<Pill> Pills { get; private set; }
        public static IList<int> Containers { get; set; }

        static DataProvider()
        {
            FillPeriodicities();
            FillDailyPeriodicities();
            FillWeeklyPeriodicities();
            FillPills();
            FillContainers();
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

        private static void FillWeeklyPeriodicities()
        {
            WeeklyIntervals = new List<WeeklyInterval>()
            {
                new WeeklyInterval { Id = 1,  Description = "Lunes" },
                new WeeklyInterval { Id = 2,  Description = "Martes" },
                new WeeklyInterval { Id = 3,  Description = "Miércoles" },
                new WeeklyInterval { Id = 4,  Description = "Jueves" },
                new WeeklyInterval { Id = 5,  Description = "Viernes" },
                new WeeklyInterval { Id = 6,  Description = "Sábado" },
                new WeeklyInterval { Id = 7,  Description = "Domingo" }
            };
        }
    }
}
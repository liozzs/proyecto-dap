using DAP.Mobile.Models;
using System.Collections.Generic;

namespace DAP.Mobile.Services
{
    public static class DataProvider
    {
        public static IList<Periodicity> DailyPeriodicities { get; private set; }
        public static IList<Periodicity> Periodicities { get; private set; }
        public static IList<Pill> Pills { get; private set; }

        static DataProvider()
        {
            FillPeriodicities();
            FillDailyPeriodicities();
            FillPills();
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
            DailyPeriodicities = new List<Periodicity>()
            {
                new Periodicity { Id = 1, Description = "Cada 6 hs" },
                new Periodicity { Id = 2, Description = "Cada 8 hs" },
                new Periodicity { Id = 3, Description = "Cada 12 hs" },
                new Periodicity { Id = 4, Description = "Cada 24 hs" }
            };
        }
    }
}

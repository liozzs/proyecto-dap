using DAP.Mobile.Models;
using System;
using System.Collections.Generic;

namespace DAP.Mobile.Services
{
    public static class PlanificationBuilder
    {
        private static Planification planification;

        public static void Create(PlanificationType type, Pill pill, DateTime startDate)
        {
            planification = new Planification
            {
                Type = type,
                StartDate = startDate,
                Pill = pill
            };
        }

        public static void SetInterval(DateTime startTime, int? interval, IList<bool> days)
        {
            planification.StartTime = startTime;
            planification.Interval = interval;
            planification.Days = days;
        }

        internal static void SetAction(PlanificationAction action)
        {
            planification.Action = action;
        }

        internal static Planification Build()
        {
            return planification;
        }
    }
}
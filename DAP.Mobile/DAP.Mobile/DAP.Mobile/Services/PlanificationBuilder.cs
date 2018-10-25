using DAP.Mobile.Models;
using System;
using System.Collections.Generic;

namespace DAP.Mobile.Services
{
    public static class PlanificationBuilder
    {
        private static PlanificationType _type;
        private static DateTime _startDate;
        private static DateTime _startTime;
        private static Pill _pill;
        private static int? _interval;
        private static IList<bool> _days;
        private static PlanificationAction _action;
        private static int _criticalStock;
        private static int _qtyToDispense;

        public static void SetInterval(TimeSpan startTime, int? interval, IList<bool> days)
        {
            _startTime = new DateTime(startTime.Ticks);
            _interval = interval;
            _days = days;
        }

        public static void SetAction(PlanificationAction action)
        {
            _action = action;
        }

        public static void SetType(PlanificationType type)
        {
            _type = type;
        }

        public static void SetPill(Pill pill)
        {
            _pill = pill;
        }

        public static void SetStartDate(DateTime startDate)
        {
            _startDate = startDate;
        }

        public static void SetCriticalStock(int criticalStock)
        {
            _criticalStock = criticalStock;
        }

        public static void SetQtyToDispense(int qtyToDispense)
        {
            _qtyToDispense = qtyToDispense;
        }

        public static Planification Build()
        {
            return new Planification
            {
                Type = _type,
                StartDate = _startDate,
                StartTime = _startTime,
                Pill = _pill,
                Interval = _interval,
                Days = _days,
                Action = _action,
                CriticalStock = _criticalStock,
                QtyToDispense = _qtyToDispense
            };
        }

    }
}
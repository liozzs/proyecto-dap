using DAP.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAP.Mobile.Services
{
    public static class PlanificationBuilder
    {
        private static PlanificationType _planificationType;
        private static DateTime _startDate;
        private static DateTime _startTime;
        private static Pill _pill;
        private static int _interval;
        private static IList<bool> _days;
        private static PlanificationAction _action;
        private static int _criticalStock;
        private static int _qtyToDispense;
        private static int _id;

        public static void SetInterval(TimeSpan startTime, int interval, IList<bool> days)
        {
            _startTime = new DateTime(startTime.Ticks);
            _interval = interval;
            _days = days;
        }

        public static void SetAction(PlanificationAction action)
        {
            _action = action;
        }

        public static void SetPlanificationType(PlanificationType type)
        {
            _planificationType = type;
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
                Id = _id,
                Type = Convert.ToInt32(_planificationType),
                StartDate = _startDate.ToString("yyyyMMdd"),
                StartTime = _startTime.ToString("HHmmss"),
                PillId = _pill.Id,
                PillName = _pill.Name,
                Container = _pill.Container,
                Interval = _interval,
                Days = _days != null ? string.Join("", _days.Select(b => b ? "1" : "0")) : "0000000",
                ActionId = _action.Id,
                ActionDescription = _action.Description,
                CriticalStock = _criticalStock,
                QtyToDispense = _qtyToDispense
            };
        }

        public static void SetId(int id)
        {
            _id = id;
        }
    }
}
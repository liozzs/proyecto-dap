using System;
using System.Collections.Generic;
using System.Linq;

namespace DAP.Mobile.Models
{
    public enum PlanificationType
    {
        Daily = 0,
        Weekly = 1,
        Custom = 2
    }

    public class Planification : Entity
    {
        public PlanificationType Type { get; internal set; }
        public DateTime StartDate { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public Pill Pill { get; internal set; }
        public int? Interval { get; internal set; }
        public IList<bool> Days { get; internal set; }
        public PlanificationAction Action { get; internal set; }
        public int CriticalStock { get; internal set; }
        public int QtyToDispense { get; internal set; }

        public string TypeDescription
        {
            get
            {
                switch (Type)
                {
                    case PlanificationType.Daily:
                        return "Diaria";
                    case PlanificationType.Weekly:
                        return "Semanal";
                    case PlanificationType.Custom:
                        return "Personalizada";
                    default:
                        return "";
                }
            }
        }

        public dynamic ToJson()
        {
            return new
            {
                StartTime = $"{StartDate:yyyyMMdd}{StartTime:HHmmss}",
                Interval = Interval.GetValueOrDefault() * 60 * 60,
                Quantity = QtyToDispense,
                CriticalStock,
                Periodicity = Convert.ToInt32(Type),
                Days = string.Join("", Days.Select(b => b? "1" : "0")),
                Block = Action.Id,
                PlateId = Pill.Container * 100
            };
        }
    }
}
using System;
using System.Collections.Generic;

namespace DAP.Mobile.Models
{
    public enum PlanificationType
    {
        Daily = 1,
        Weekly = 2,
        Custom = 3
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
    }
}
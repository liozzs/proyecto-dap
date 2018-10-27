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
        public int Type { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public int PillId { get; set; }
        [SQLite.Ignore]
        public string PillName { get; set; }
        public int Interval { get; set; }
        public string Days { get; set; }
        public int ActionId { get; set; }
        [SQLite.Ignore]
        public string ActionDescription { get; set; }
        public int CriticalStock { get; set; }
        public int QtyToDispense { get; set; }
        [SQLite.Ignore]
        public int Container { get; set; }

        [SQLite.Ignore]
        public string TypeDescription
        {
            get
            {
                switch (Type)
                {
                    case (int)PlanificationType.Daily:
                        return "Diaria";
                    case (int)PlanificationType.Weekly:
                        return "Semanal";
                    case (int)PlanificationType.Custom:
                        return "Personalizada";
                    default:
                        return "";
                }
            }
        }

    }
}
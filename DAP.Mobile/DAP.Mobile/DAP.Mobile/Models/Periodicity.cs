using Prism.Mvvm;

namespace DAP.Mobile.Models
{
    public class Periodicity
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string NextPage { get; set; }
    }

    public class DailyInterval
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class WeeklyInterval : BindableBase
    {
        public int Id { get; set; }
        public string Description { get; set; }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
    }
}
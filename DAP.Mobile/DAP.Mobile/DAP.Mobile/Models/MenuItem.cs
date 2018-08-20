using Prism.Commands;

namespace DAP.Mobile.Models
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
        public DelegateCommand Command { get; set; }
    }
}
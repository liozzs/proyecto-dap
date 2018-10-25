using System;

namespace DAP.Mobile.Models
{
    public class Notification : Entity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
    }
}
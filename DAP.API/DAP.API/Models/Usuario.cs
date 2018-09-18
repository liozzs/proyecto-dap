using System.Collections.Generic;

namespace DAP.API.Models
{
    public class Usuario
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Email { get; set; }

        public virtual List<Dispenser> Dispensers { get; set; }
    }
}

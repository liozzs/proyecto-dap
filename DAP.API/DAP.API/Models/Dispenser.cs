using System.Collections.Generic;

namespace DAP.API.Models
{
    public class Dispenser
    {
        public int ID { get; set; }
        public string DireccionMAC { get; set; }
        public string Nombre { get; set; }

        public virtual List<Usuario> Usuarios { get; set; }
    }
}

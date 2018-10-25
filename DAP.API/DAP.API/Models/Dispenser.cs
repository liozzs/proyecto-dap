using System;
using System.Collections.Generic;

namespace DAP.API.Models
{
    public class Dispenser: IEquatable<Dispenser>
    {
        public int ID { get; set; }
        public string DireccionMAC { get; set; }
        public string Nombre { get; set; }

        public virtual List<Usuario> Usuarios { get; set; }
        public virtual List<DispenserMensaje> Mensajes { get; set; }

        public bool Equals(Dispenser dispenser)
        {
            //var dispenser = obj as Dispenser;
            return dispenser != null &&
                   ID == dispenser.ID;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAP.API.Models
{
    public class CargaStockMensaje
    {
        public string DireccionMAC { get; set; }
        public int Receptaculo { get; set; }
        public string Pastilla { get; set; }
        public int Stock { get; set; }
    }
}

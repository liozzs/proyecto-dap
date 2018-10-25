using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAP.API.Models
{
    public class PlanificacionMensaje
    {
        public string DireccionMAC { get; set; }
        public int Receptaculo { get; set; }
        public string HorarioInicio { get; set; }
        public string Intervalo { get; set; }
        public int Cantidad { get; set; }
        public int StockCritico { get; set; }
        public string Periodicidad { get; set; }
        public string Dias { get; set; }
        public string Bloqueo { get; set; }
    }
}

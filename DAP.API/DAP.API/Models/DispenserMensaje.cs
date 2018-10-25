using System;

namespace DAP.API.Models
{
    public enum CodigoError
    {
        FALTA_DE_PASTILLAS  = 1,
        LIMITE_DE_TIEMPO    = 2,
        STOCK_CRITICO       = 3,
        BOTON_NO_PRESIONADO = 4,
        VASO_NO_RETIRADO    = 5,
        VASO_NO_DEVUELTO    = 6,
        BLOQUEO_RECIPIENTE  = 7
    }
    
    public class DispenserMensaje: IEquatable<DispenserMensaje>
    {
        public int ID { get; set; }
        public int DispenserID { get; set; }
        public CodigoError Codigo { get; set; }
        public string Mensaje { get; set; }
        public int Receptaculo { get; set; }
        public string Pastilla { get; set; }
        public string Horario { get; set; }
        public int CantidadRestante { get; set; }

        public virtual Dispenser Dispenser { get; set; }
        public virtual string DireccionMAC { get; set; }

        public bool Equals(DispenserMensaje other)
        {
            return other != null && ID == other.ID;
        }
    }
}

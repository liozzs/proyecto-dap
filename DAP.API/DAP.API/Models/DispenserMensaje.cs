namespace DAP.API.Models
{
    public enum CodigoError
    {
        E001 = 1,
        E002 = 2,
        E003 = 3,
        E004 = 4,
        E005 = 5
    }

    public class DispenserMensaje
    {
        public string DireccionMAC { get; set; }
        public CodigoError Codigo { get; set; }
        public int Receptaculo { get; set; }
        public string Pastilla { get; set; }
        public string Horario { get; set; }
        public int CantidadRestante { get; set; }
    }
}

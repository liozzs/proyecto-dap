namespace DAP.API.Models
{
    public class UsuarioDispenser
    {
        public int UsuarioID { get; set; }
        public int DispenserID { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Dispenser Dispenser { get; set; }
    }
}
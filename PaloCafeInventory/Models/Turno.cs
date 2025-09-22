using System.ComponentModel.DataAnnotations;

namespace PaloCafeInventory.Models
{
    public class Turno
    {
        public int Id { get; set; }
        
        public int UsuarioId { get; set; }
        
        public DateTime HoraInicio { get; set; } = DateTime.Now;
        
        public DateTime? HoraCierre { get; set; }
        
        public decimal TotalVendido { get; set; }
        
        public bool Cerrado { get; set; } = false;
        
        public int NumeroTurno { get; set; } // 1 o 2 para máximo 2 turnos por día
        
        public DateTime Fecha { get; set; } = DateTime.Today;
        
        [StringLength(200)]
        public string Observaciones { get; set; } = string.Empty;
        
        // Propiedades de navegación
        public virtual Usuario? Usuario { get; set; }
        public virtual List<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    }
}
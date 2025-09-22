using System.ComponentModel.DataAnnotations;

namespace PaloCafeInventory.Models
{
    public class CierreDiario
    {
        public int Id { get; set; }
        
        public DateTime Fecha { get; set; } = DateTime.Today;
        
        public decimal TotalGeneral { get; set; }
        
        public int GeneradoPor { get; set; }
        
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string Resumen { get; set; } = string.Empty;
        
        public int CantidadTurnos { get; set; }
        
        // Propiedades de navegación
        public virtual Usuario? UsuarioGenerador { get; set; }
    }
}
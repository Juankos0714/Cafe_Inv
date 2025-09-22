using System.ComponentModel.DataAnnotations;

namespace PaloCafeInventory.Models
{
    public class Movimiento
    {
        public int Id { get; set; }
        
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        public TipoMovimiento Tipo { get; set; }
        
        public int ProductoId { get; set; }
        
        public int Cantidad { get; set; }
        
        public decimal PrecioUnitario { get; set; }
        
        public decimal Total { get; set; }
        
        public int UsuarioId { get; set; }
        
        public int? TurnoId { get; set; }
        
        [StringLength(200)]
        public string Observaciones { get; set; } = string.Empty;
        
        // Propiedades de navegación
        public virtual Producto? Producto { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public virtual Turno? Turno { get; set; }
    }

    public enum TipoMovimiento
    {
        Venta = 1,
        Entrada = 2,
        Ajuste = 3,
        Devolucion = 4
    }
}
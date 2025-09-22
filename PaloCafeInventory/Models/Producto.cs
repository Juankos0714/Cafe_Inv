using System.ComponentModel.DataAnnotations;

namespace PaloCafeInventory.Models
{
    public class Producto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;
        
        public decimal Precio { get; set; }
        
        public int StockActual { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
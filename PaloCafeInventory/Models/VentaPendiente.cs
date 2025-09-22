using Newtonsoft.Json;

namespace PaloCafeInventory.Models
{
    public class VentaPendiente
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public List<ProductoVenta> Productos { get; set; } = new List<ProductoVenta>();
        
        public decimal Total { get; set; }
        
        public bool Procesada { get; set; } = false;
        
        public int UsuarioId { get; set; }
        
        [JsonIgnore]
        public string TotalFormateado => $"${Total:N0}";
    }

    public class ProductoVenta
    {
        public string Codigo { get; set; } = string.Empty;
        
        public string Nombre { get; set; } = string.Empty;
        
        public int Cantidad { get; set; }
        
        public decimal Precio { get; set; }
        
        public decimal Subtotal { get; set; }
        
        [JsonIgnore]
        public string SubtotalFormateado => $"${Subtotal:N0}";
        
        [JsonIgnore]
        public string PrecioFormateado => $"${Precio:N0}";
    }
}
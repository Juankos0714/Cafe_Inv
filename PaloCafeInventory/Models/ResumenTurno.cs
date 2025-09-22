namespace PaloCafeInventory.Models
{
    public class ResumenTurno
    {
        public int TurnoId { get; set; }
        
        public int NumeroTurno { get; set; }
        
        public DateTime Fecha { get; set; }
        
        public DateTime HoraInicio { get; set; }
        
        public DateTime HoraCierre { get; set; }
        
        public string Usuario { get; set; } = string.Empty;
        
        public List<VentaTurno> Ventas { get; set; } = new List<VentaTurno>();
        
        public int TotalVentas { get; set; }
        
        public decimal TotalIngresos { get; set; }
        
        public Dictionary<string, int> ProductosMasVendidos { get; set; } = new Dictionary<string, int>();
    }

    public class VentaTurno
    {
        public DateTime Hora { get; set; }
        
        public string Producto { get; set; } = string.Empty;
        
        public int Cantidad { get; set; }
        
        public decimal PrecioUnitario { get; set; }
        
        public decimal Total { get; set; }
    }
}
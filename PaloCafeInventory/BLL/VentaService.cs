using PaloCafeInventory.DAL;
using PaloCafeInventory.Models;
using Newtonsoft.Json;

namespace PaloCafeInventory.BLL
{
    public class VentaService
    {
        private readonly MovimientoRepository _movimientoRepository;
        private readonly ProductoService _productoService;
        private readonly string _ventasPendientesPath;
        
        public VentaService()
        {
            _movimientoRepository = new MovimientoRepository();
            _productoService = new ProductoService();
            _ventasPendientesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ventas_pendientes.json");
        }

        public async Task<bool> ValidarVenta(List<ProductoVenta> productos)
        {
            foreach (var producto in productos)
            {
                var disponible = await _productoService.ValidarStock(producto.Codigo, producto.Cantidad);
                if (!disponible)
                    return false;
            }
            return true;
        }

        public async Task<bool> ProcesarVenta(List<ProductoVenta> productos, int usuarioId, int turnoId)
        {
            // Validar stock antes de procesar
            if (!await ValidarVenta(productos))
                return false;

            try
            {
                await _movimientoRepository.RegistrarVenta(productos, usuarioId, turnoId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void GuardarVentaPendiente(VentaPendiente venta)
        {
            var ventasPendientes = CargarVentasPendientes();
            ventasPendientes.RemoveAll(v => v.Id == venta.Id); // Evitar duplicados
            ventasPendientes.Add(venta);
            
            var json = JsonConvert.SerializeObject(ventasPendientes, Formatting.Indented);
            File.WriteAllText(_ventasPendientesPath, json);
        }

        public List<VentaPendiente> CargarVentasPendientes()
        {
            try
            {
                if (File.Exists(_ventasPendientesPath))
                {
                    var json = File.ReadAllText(_ventasPendientesPath);
                    var ventas = JsonConvert.DeserializeObject<List<VentaPendiente>>(json) ?? new List<VentaPendiente>();
                    return ventas.Where(v => !v.Procesada).ToList();
                }
            }
            catch
            {
                // Si hay error al leer, retornar lista vacía
            }
            
            return new List<VentaPendiente>();
        }

        public void MarcarVentaComoProcesada(string ventaId)
        {
            var ventasPendientes = CargarVentasPendientes();
            var venta = ventasPendientes.FirstOrDefault(v => v.Id == ventaId);
            
            if (venta != null)
            {
                venta.Procesada = true;
                var json = JsonConvert.SerializeObject(ventasPendientes, Formatting.Indented);
                File.WriteAllText(_ventasPendientesPath, json);
            }
        }

        public void EliminarVentasPendientes()
        {
            if (File.Exists(_ventasPendientesPath))
            {
                File.Delete(_ventasPendientesPath);
            }
        }

        public async Task<bool> ProcesarVentasPendientes(int usuarioId, int turnoId)
        {
            var ventasPendientes = CargarVentasPendientes();
            var procesadas = 0;

            foreach (var venta in ventasPendientes.Where(v => v.UsuarioId == usuarioId))
            {
                try
                {
                    if (await ProcesarVenta(venta.Productos, usuarioId, turnoId))
                    {
                        MarcarVentaComoProcesada(venta.Id);
                        procesadas++;
                    }
                }
                catch
                {
                    // Continuar con la siguiente venta si una falla
                    continue;
                }
            }

            return procesadas > 0;
        }

        public decimal CalcularTotalVenta(List<ProductoVenta> productos)
        {
            return productos.Sum(p => p.Subtotal);
        }
    }
}
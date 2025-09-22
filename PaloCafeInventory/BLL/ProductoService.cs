using PaloCafeInventory.DAL;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.BLL
{
    public class ProductoService
    {
        private readonly ProductoRepository _repository;
        
        public ProductoService()
        {
            _repository = new ProductoRepository();
        }

        public async Task<List<Producto>> ObtenerTodos()
        {
            return await _repository.ObtenerTodos();
        }

        public async Task<List<Producto>> BuscarProductos(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return await _repository.ObtenerTodos();

            return await _repository.BuscarPorNombre(filtro);
        }

        public async Task<Producto?> ObtenerPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            return await _repository.ObtenerPorCodigo(codigo);
        }

        public async Task<bool> CrearProducto(string codigo, string nombre, string categoria, decimal precio, int stockInicial)
        {
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre) || precio <= 0)
                return false;

            // Verificar que el código no exista
            var existente = await _repository.ObtenerPorCodigo(codigo);
            if (existente != null)
                return false;

            var producto = new Producto
            {
                Codigo = codigo.Trim().ToUpper(),
                Nombre = nombre.Trim(),
                Categoria = categoria?.Trim() ?? string.Empty,
                Precio = precio,
                StockActual = stockInicial,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            return await _repository.Crear(producto);
        }

        public async Task<bool> ActualizarProducto(Producto producto)
        {
            if (producto == null || string.IsNullOrWhiteSpace(producto.Codigo) || 
                string.IsNullOrWhiteSpace(producto.Nombre) || producto.Precio <= 0)
                return false;

            return await _repository.Actualizar(producto);
        }

        public async Task<bool> ValidarStock(string codigo, int cantidadRequerida)
        {
            var producto = await _repository.ObtenerPorCodigo(codigo);
            return producto != null && producto.StockActual >= cantidadRequerida;
        }

        public async Task<bool> AjustarStock(int productoId, int nuevaCantidad, string motivo = "")
        {
            var producto = await _repository.ObtenerPorId(productoId);
            if (producto == null)
                return false;

            return await _repository.ActualizarStock(productoId, nuevaCantidad);
        }

        public async Task<List<Producto>> ObtenerProductosBajoStock(int limite = 10)
        {
            var productos = await _repository.ObtenerTodos();
            return productos.Where(p => p.StockActual <= limite).ToList();
        }
    }
}
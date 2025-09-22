using System.Data.SQLite;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.DAL
{
    public class MovimientoRepository
    {
        public async Task<bool> RegistrarVenta(List<ProductoVenta> productos, int usuarioId, int turnoId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var producto in productos)
                {
                    // Obtener el producto de la base de datos
                    var productoRepo = new ProductoRepository();
                    var productoBD = await productoRepo.ObtenerPorCodigo(producto.Codigo);
                    
                    if (productoBD == null)
                        throw new InvalidOperationException($"Producto {producto.Codigo} no encontrado");

                    if (productoBD.StockActual < producto.Cantidad)
                        throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}");

                    // Registrar el movimiento
                    var insertMovimiento = @"
                        INSERT INTO Movimientos (Fecha, Tipo, ProductoId, Cantidad, PrecioUnitario, Total, UsuarioId, TurnoId) 
                        VALUES (@fecha, @tipo, @productoId, @cantidad, @precio, @total, @usuarioId, @turnoId)";

                    using var movCommand = new SQLiteCommand(insertMovimiento, connection, transaction);
                    movCommand.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    movCommand.Parameters.AddWithValue("@tipo", (int)TipoMovimiento.Venta);
                    movCommand.Parameters.AddWithValue("@productoId", productoBD.Id);
                    movCommand.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                    movCommand.Parameters.AddWithValue("@precio", producto.Precio);
                    movCommand.Parameters.AddWithValue("@total", producto.Subtotal);
                    movCommand.Parameters.AddWithValue("@usuarioId", usuarioId);
                    movCommand.Parameters.AddWithValue("@turnoId", turnoId);

                    await movCommand.ExecuteNonQueryAsync();

                    // Actualizar stock
                    var updateStock = "UPDATE Productos SET StockActual = StockActual - @cantidad WHERE Id = @id";
                    using var stockCommand = new SQLiteCommand(updateStock, connection, transaction);
                    stockCommand.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                    stockCommand.Parameters.AddWithValue("@id", productoBD.Id);

                    await stockCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<Movimiento>> ObtenerMovimientosPorTurno(int turnoId)
        {
            var movimientos = new List<Movimiento>();
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT m.Id, m.Fecha, m.Tipo, m.ProductoId, m.Cantidad, 
                       m.PrecioUnitario, m.Total, m.UsuarioId, m.TurnoId, m.Observaciones,
                       p.Nombre as ProductoNombre, p.Codigo as ProductoCodigo,
                       u.Nombre as UsuarioNombre
                FROM Movimientos m
                INNER JOIN Productos p ON m.ProductoId = p.Id
                INNER JOIN Usuarios u ON m.UsuarioId = u.Id
                WHERE m.TurnoId = @turnoId
                ORDER BY m.Fecha DESC";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@turnoId", turnoId);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                movimientos.Add(new Movimiento
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Fecha = DateTime.Parse(reader["Fecha"].ToString()!),
                    Tipo = (TipoMovimiento)Convert.ToInt32(reader["Tipo"]),
                    ProductoId = Convert.ToInt32(reader["ProductoId"]),
                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                    Total = Convert.ToDecimal(reader["Total"]),
                    UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                    TurnoId = Convert.ToInt32(reader["TurnoId"]),
                    Observaciones = reader["Observaciones"].ToString()!,
                    Producto = new Producto 
                    { 
                        Nombre = reader["ProductoNombre"].ToString()!, 
                        Codigo = reader["ProductoCodigo"].ToString()! 
                    },
                    Usuario = new Usuario { Nombre = reader["UsuarioNombre"].ToString()! }
                });
            }

            return movimientos;
        }

        public async Task<decimal> CalcularTotalVendidoTurno(int turnoId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT COALESCE(SUM(Total), 0) 
                FROM Movimientos 
                WHERE TurnoId = @turnoId AND Tipo = 1";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@turnoId", turnoId);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToDecimal(result);
        }
    }
}
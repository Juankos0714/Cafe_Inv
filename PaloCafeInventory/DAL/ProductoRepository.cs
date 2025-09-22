using System.Data.SQLite;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.DAL
{
    public class ProductoRepository
    {
        public async Task<List<Producto>> ObtenerTodos()
        {
            var productos = new List<Producto>();
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Codigo, Nombre, Categoria, Precio, StockActual, Activo, FechaCreacion 
                FROM Productos 
                WHERE Activo = 1 
                ORDER BY Nombre";

            using var command = new SQLiteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                productos.Add(new Producto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Codigo = reader["Codigo"].ToString()!,
                    Nombre = reader["Nombre"].ToString()!,
                    Categoria = reader["Categoria"].ToString()!,
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    StockActual = Convert.ToInt32(reader["StockActual"]),
                    Activo = Convert.ToBoolean(reader["Activo"]),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()!)
                });
            }

            return productos;
        }

        public async Task<List<Producto>> BuscarPorNombre(string nombre)
        {
            var productos = new List<Producto>();
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Codigo, Nombre, Categoria, Precio, StockActual, Activo, FechaCreacion 
                FROM Productos 
                WHERE Activo = 1 AND (Nombre LIKE @nombre OR Codigo LIKE @nombre)
                ORDER BY Nombre";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@nombre", $"%{nombre}%");
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                productos.Add(new Producto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Codigo = reader["Codigo"].ToString()!,
                    Nombre = reader["Nombre"].ToString()!,
                    Categoria = reader["Categoria"].ToString()!,
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    StockActual = Convert.ToInt32(reader["StockActual"]),
                    Activo = Convert.ToBoolean(reader["Activo"]),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()!)
                });
            }

            return productos;
        }

        public async Task<Producto?> ObtenerPorId(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Codigo, Nombre, Categoria, Precio, StockActual, Activo, FechaCreacion 
                FROM Productos 
                WHERE Id = @id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Producto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Codigo = reader["Codigo"].ToString()!,
                    Nombre = reader["Nombre"].ToString()!,
                    Categoria = reader["Categoria"].ToString()!,
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    StockActual = Convert.ToInt32(reader["StockActual"]),
                    Activo = Convert.ToBoolean(reader["Activo"]),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()!)
                };
            }

            return null;
        }

        public async Task<Producto?> ObtenerPorCodigo(string codigo)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Codigo, Nombre, Categoria, Precio, StockActual, Activo, FechaCreacion 
                FROM Productos 
                WHERE Codigo = @codigo AND Activo = 1";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@codigo", codigo);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Producto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Codigo = reader["Codigo"].ToString()!,
                    Nombre = reader["Nombre"].ToString()!,
                    Categoria = reader["Categoria"].ToString()!,
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    StockActual = Convert.ToInt32(reader["StockActual"]),
                    Activo = Convert.ToBoolean(reader["Activo"]),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()!)
                };
            }

            return null;
        }

        public async Task<bool> Crear(Producto producto)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO Productos (Codigo, Nombre, Categoria, Precio, StockActual, Activo, FechaCreacion) 
                    VALUES (@codigo, @nombre, @categoria, @precio, @stock, @activo, @fecha)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@codigo", producto.Codigo);
                command.Parameters.AddWithValue("@nombre", producto.Nombre);
                command.Parameters.AddWithValue("@categoria", producto.Categoria);
                command.Parameters.AddWithValue("@precio", producto.Precio);
                command.Parameters.AddWithValue("@stock", producto.StockActual);
                command.Parameters.AddWithValue("@activo", producto.Activo);
                command.Parameters.AddWithValue("@fecha", producto.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"));

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Producto producto)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    UPDATE Productos 
                    SET Codigo = @codigo, Nombre = @nombre, Categoria = @categoria, 
                        Precio = @precio, StockActual = @stock, Activo = @activo 
                    WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@codigo", producto.Codigo);
                command.Parameters.AddWithValue("@nombre", producto.Nombre);
                command.Parameters.AddWithValue("@categoria", producto.Categoria);
                command.Parameters.AddWithValue("@precio", producto.Precio);
                command.Parameters.AddWithValue("@stock", producto.StockActual);
                command.Parameters.AddWithValue("@activo", producto.Activo);
                command.Parameters.AddWithValue("@id", producto.Id);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarStock(int productoId, int nuevaCantidad)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "UPDATE Productos SET StockActual = @stock WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@stock", nuevaCantidad);
                command.Parameters.AddWithValue("@id", productoId);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
using System.Data.SQLite;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.DAL
{
    public class TurnoRepository
    {
        public async Task<Turno?> ObtenerTurnoActivo(int usuarioId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT t.Id, t.UsuarioId, t.HoraInicio, t.HoraCierre, t.TotalVendido, 
                       t.Cerrado, t.NumeroTurno, t.Fecha, t.Observaciones,
                       u.Nombre as UsuarioNombre
                FROM Turnos t
                INNER JOIN Usuarios u ON t.UsuarioId = u.Id
                WHERE t.UsuarioId = @usuarioId AND t.Cerrado = 0 
                ORDER BY t.Id DESC LIMIT 1";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Turno
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                    HoraInicio = DateTime.Parse(reader["HoraInicio"].ToString()!),
                    HoraCierre = reader["HoraCierre"] == DBNull.Value ? null : DateTime.Parse(reader["HoraCierre"].ToString()!),
                    TotalVendido = Convert.ToDecimal(reader["TotalVendido"]),
                    Cerrado = Convert.ToBoolean(reader["Cerrado"]),
                    NumeroTurno = Convert.ToInt32(reader["NumeroTurno"]),
                    Fecha = DateTime.Parse(reader["Fecha"].ToString()!),
                    Observaciones = reader["Observaciones"].ToString()!,
                    Usuario = new Usuario { Nombre = reader["UsuarioNombre"].ToString()! }
                };
            }

            return null;
        }

        public async Task<int> ContarTurnosDelDia(DateTime fecha)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM Turnos WHERE Fecha = @fecha";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> IniciarTurno(int usuarioId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var fecha = DateTime.Today;
            var turnosHoy = await ContarTurnosDelDia(fecha);

            if (turnosHoy >= 2)
            {
                throw new InvalidOperationException("Ya se han registrado los 2 turnos máximos para el día");
            }

            var numeroTurno = turnosHoy + 1;

            var query = @"
                INSERT INTO Turnos (UsuarioId, HoraInicio, NumeroTurno, Fecha, Cerrado) 
                VALUES (@usuarioId, @horaInicio, @numeroTurno, @fecha, 0)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);
            command.Parameters.AddWithValue("@horaInicio", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@numeroTurno", numeroTurno);
            command.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));

            await command.ExecuteNonQueryAsync();

            // Obtener el ID del turno recién creado
            var lastIdQuery = "SELECT last_insert_rowid()";
            using var lastIdCommand = new SQLiteCommand(lastIdQuery, connection);
            var turnoId = Convert.ToInt32(await lastIdCommand.ExecuteScalarAsync());

            return turnoId;
        }

        public async Task<bool> CerrarTurno(int turnoId, decimal totalVendido, string observaciones = "")
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    UPDATE Turnos 
                    SET HoraCierre = @horaCierre, TotalVendido = @total, 
                        Cerrado = 1, Observaciones = @observaciones 
                    WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@horaCierre", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@total", totalVendido);
                command.Parameters.AddWithValue("@observaciones", observaciones);
                command.Parameters.AddWithValue("@id", turnoId);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Turno>> ObtenerTurnosPorFecha(DateTime fecha)
        {
            var turnos = new List<Turno>();
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT t.Id, t.UsuarioId, t.HoraInicio, t.HoraCierre, t.TotalVendido, 
                       t.Cerrado, t.NumeroTurno, t.Fecha, t.Observaciones,
                       u.Nombre as UsuarioNombre
                FROM Turnos t
                INNER JOIN Usuarios u ON t.UsuarioId = u.Id
                WHERE t.Fecha = @fecha 
                ORDER BY t.NumeroTurno";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@fecha", fecha.ToString("yyyy-MM-dd"));
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                turnos.Add(new Turno
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                    HoraInicio = DateTime.Parse(reader["HoraInicio"].ToString()!),
                    HoraCierre = reader["HoraCierre"] == DBNull.Value ? null : DateTime.Parse(reader["HoraCierre"].ToString()!),
                    TotalVendido = Convert.ToDecimal(reader["TotalVendido"]),
                    Cerrado = Convert.ToBoolean(reader["Cerrado"]),
                    NumeroTurno = Convert.ToInt32(reader["NumeroTurno"]),
                    Fecha = DateTime.Parse(reader["Fecha"].ToString()!),
                    Observaciones = reader["Observaciones"].ToString()!,
                    Usuario = new Usuario { Nombre = reader["UsuarioNombre"].ToString()! }
                });
            }

            return turnos;
        }

        public async Task<ResumenTurno?> GenerarResumenTurno(int turnoId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Obtener información del turno
            var turnoQuery = @"
                SELECT t.Id, t.NumeroTurno, t.Fecha, t.HoraInicio, t.HoraCierre, 
                       t.TotalVendido, u.Nombre as UsuarioNombre
                FROM Turnos t
                INNER JOIN Usuarios u ON t.UsuarioId = u.Id
                WHERE t.Id = @turnoId";

            using var turnoCommand = new SQLiteCommand(turnoQuery, connection);
            turnoCommand.Parameters.AddWithValue("@turnoId", turnoId);
            using var turnoReader = await turnoCommand.ExecuteReaderAsync();

            if (!await turnoReader.ReadAsync())
                return null;

            var resumen = new ResumenTurno
            {
                TurnoId = Convert.ToInt32(turnoReader["Id"]),
                NumeroTurno = Convert.ToInt32(turnoReader["NumeroTurno"]),
                Fecha = DateTime.Parse(turnoReader["Fecha"].ToString()!),
                HoraInicio = DateTime.Parse(turnoReader["HoraInicio"].ToString()!),
                HoraCierre = turnoReader["HoraCierre"] == DBNull.Value ? DateTime.Now : DateTime.Parse(turnoReader["HoraCierre"].ToString()!),
                Usuario = turnoReader["UsuarioNombre"].ToString()!,
                TotalIngresos = Convert.ToDecimal(turnoReader["TotalVendido"])
            };

            // Obtener ventas del turno
            var ventasQuery = @"
                SELECT m.Fecha, p.Nombre as ProductoNombre, m.Cantidad, 
                       m.PrecioUnitario, m.Total
                FROM Movimientos m
                INNER JOIN Productos p ON m.ProductoId = p.Id
                WHERE m.TurnoId = @turnoId AND m.Tipo = 1
                ORDER BY m.Fecha";

            using var ventasCommand = new SQLiteCommand(ventasQuery, connection);
            ventasCommand.Parameters.AddWithValue("@turnoId", turnoId);
            using var ventasReader = await ventasCommand.ExecuteReaderAsync();

            while (await ventasReader.ReadAsync())
            {
                var venta = new VentaTurno
                {
                    Hora = DateTime.Parse(ventasReader["Fecha"].ToString()!),
                    Producto = ventasReader["ProductoNombre"].ToString()!,
                    Cantidad = Convert.ToInt32(ventasReader["Cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(ventasReader["PrecioUnitario"]),
                    Total = Convert.ToDecimal(ventasReader["Total"])
                };

                resumen.Ventas.Add(venta);

                // Actualizar productos más vendidos
                if (resumen.ProductosMasVendidos.ContainsKey(venta.Producto))
                    resumen.ProductosMasVendidos[venta.Producto] += venta.Cantidad;
                else
                    resumen.ProductosMasVendidos[venta.Producto] = venta.Cantidad;
            }

            resumen.TotalVentas = resumen.Ventas.Count;
            return resumen;
        }
    }
}
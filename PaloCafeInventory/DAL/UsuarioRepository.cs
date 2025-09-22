using System.Data.SQLite;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.DAL
{
    public class UsuarioRepository
    {
        public async Task<Usuario?> ValidarUsuario(string nombre, string password)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Nombre, PasswordHash, Rol, Activo, FechaCreacion 
                FROM Usuarios 
                WHERE Nombre = @nombre AND Activo = 1";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@nombre", nombre);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var hash = reader["PasswordHash"].ToString();
                if (!string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash))
                {
                    return new Usuario
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nombre = reader["Nombre"].ToString()!,
                        PasswordHash = hash,
                        Rol = (Roles)Convert.ToInt32(reader["Rol"]),
                        Activo = Convert.ToBoolean(reader["Activo"]),
                        FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()!)
                    };
                }
            }

            return null;
        }

        public async Task<List<Usuario>> ObtenerTodos()
        {
            var usuarios = new List<Usuario>();
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Nombre, PasswordHash, Rol, Activo, FechaCreacion 
                FROM Usuarios 
                ORDER BY Nombre";

            using var command = new SQLiteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usuarios.Add(new Usuario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString()!,
                    PasswordHash = reader["PasswordHash"].ToString()!,
                    Rol = (Roles)Convert.ToInt32(reader["Rol"]),
                    Activo = Convert.ToBoolean(reader["Activo"]),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()!)
                });
            }

            return usuarios;
        }

        public async Task<bool> Crear(Usuario usuario)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO Usuarios (Nombre, PasswordHash, Rol, Activo, FechaCreacion) 
                    VALUES (@nombre, @hash, @rol, @activo, @fecha)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash));
                command.Parameters.AddWithValue("@rol", (int)usuario.Rol);
                command.Parameters.AddWithValue("@activo", usuario.Activo);
                command.Parameters.AddWithValue("@fecha", usuario.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"));

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Usuario usuario)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    UPDATE Usuarios 
                    SET Nombre = @nombre, Rol = @rol, Activo = @activo 
                    WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@rol", (int)usuario.Rol);
                command.Parameters.AddWithValue("@activo", usuario.Activo);
                command.Parameters.AddWithValue("@id", usuario.Id);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CambiarPassword(int usuarioId, string nuevaPassword)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "UPDATE Usuarios SET PasswordHash = @hash WHERE Id = @id";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(nuevaPassword));
                command.Parameters.AddWithValue("@id", usuarioId);

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
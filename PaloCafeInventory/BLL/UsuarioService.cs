using PaloCafeInventory.DAL;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.BLL
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _repository;
        
        public UsuarioService()
        {
            _repository = new UsuarioRepository();
        }

        public async Task<Usuario?> ValidarLogin(string nombre, string password)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(password))
                return null;

            return await _repository.ValidarUsuario(nombre, password);
        }

        public async Task<List<Usuario>> ObtenerTodos()
        {
            return await _repository.ObtenerTodos();
        }

        public async Task<bool> CrearUsuario(string nombre, string password, Roles rol)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 6)
                return false;

            var usuario = new Usuario
            {
                Nombre = nombre.Trim(),
                PasswordHash = password,
                Rol = rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            return await _repository.Crear(usuario);
        }

        public async Task<bool> ActualizarUsuario(Usuario usuario)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Nombre))
                return false;

            return await _repository.Actualizar(usuario);
        }

        public async Task<bool> CambiarPassword(int usuarioId, string passwordActual, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
                return false;

            // Aquí podrías agregar validación del password actual si es necesario
            return await _repository.CambiarPassword(usuarioId, nuevaPassword);
        }

        public bool TienePermiso(Usuario usuario, string accion)
        {
            return usuario.Rol switch
            {
                Roles.Admin => true, // Admin tiene acceso a todo
                Roles.Vendedor => accion == "VENTAS" || accion == "PRODUCTOS_CONSULTA" || accion == "CIERRE_TURNO",
                Roles.Contador => accion == "REPORTES" || accion == "PRODUCTOS_CONSULTA",
                _ => false
            };
        }
    }
}
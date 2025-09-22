using PaloCafeInventory.Models;

namespace PaloCafeInventory.Services
{
    public static class SessionManager
    {
        private static Usuario? _usuarioActual;
        private static Turno? _turnoActivo;

        public static Usuario? UsuarioActual
        {
            get => _usuarioActual;
            set => _usuarioActual = value;
        }

        public static Turno? TurnoActivo
        {
            get => _turnoActivo;
            set => _turnoActivo = value;
        }

        public static bool EstaLogueado => _usuarioActual != null;

        public static bool TieneTurnoActivo => _turnoActivo != null && !_turnoActivo.Cerrado;

        public static void CerrarSesion()
        {
            _usuarioActual = null;
            _turnoActivo = null;
        }

        public static bool TienePermiso(string accion)
        {
            if (!EstaLogueado || _usuarioActual == null)
                return false;

            return _usuarioActual.Rol switch
            {
                Roles.Admin => true,
                Roles.Vendedor => accion == "VENTAS" || accion == "PRODUCTOS_CONSULTA" || accion == "CIERRE_TURNO",
                Roles.Contador => accion == "REPORTES" || accion == "PRODUCTOS_CONSULTA",
                _ => false
            };
        }
    }
}
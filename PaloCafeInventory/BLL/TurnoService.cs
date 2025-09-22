using PaloCafeInventory.DAL;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.BLL
{
    public class TurnoService
    {
        private readonly TurnoRepository _turnoRepository;
        private readonly MovimientoRepository _movimientoRepository;
        
        public TurnoService()
        {
            _turnoRepository = new TurnoRepository();
            _movimientoRepository = new MovimientoRepository();
        }

        public async Task<Turno?> ObtenerTurnoActivo(int usuarioId)
        {
            return await _turnoRepository.ObtenerTurnoActivo(usuarioId);
        }

        public async Task<int> IniciarNuevoTurno(int usuarioId)
        {
            // Verificar que no haya turno activo
            var turnoActivo = await _turnoRepository.ObtenerTurnoActivo(usuarioId);
            if (turnoActivo != null)
                throw new InvalidOperationException("Ya existe un turno activo");

            // Verificar límite de turnos por día
            var turnosHoy = await _turnoRepository.ContarTurnosDelDia(DateTime.Today);
            if (turnosHoy >= 2)
                throw new InvalidOperationException("Ya se han registrado los 2 turnos máximos para el día");

            return await _turnoRepository.IniciarTurno(usuarioId);
        }

        public async Task<ResumenTurno?> CerrarTurno(int turnoId, string observaciones = "")
        {
            // Calcular total vendido
            var totalVendido = await _movimientoRepository.CalcularTotalVendidoTurno(turnoId);
            
            // Cerrar el turno
            var cerrado = await _turnoRepository.CerrarTurno(turnoId, totalVendido, observaciones);
            
            if (!cerrado)
                return null;

            // Generar resumen
            return await _turnoRepository.GenerarResumenTurno(turnoId);
        }

        public async Task<bool> PuedeCerrarTurno(int turnoId, int usuarioId)
        {
            var turno = await _turnoRepository.ObtenerTurnoActivo(usuarioId);
            return turno != null && turno.Id == turnoId && !turno.Cerrado;
        }

        public async Task<List<Turno>> ObtenerTurnosPorFecha(DateTime fecha)
        {
            return await _turnoRepository.ObtenerTurnosPorFecha(fecha);
        }

        public async Task<bool> PuedeIniciarTurno()
        {
            var turnosHoy = await _turnoRepository.ContarTurnosDelDia(DateTime.Today);
            return turnosHoy < 2;
        }

        public async Task<int> ObtenerNumeroTurnoActual()
        {
            var turnosHoy = await _turnoRepository.ContarTurnosDelDia(DateTime.Today);
            return turnosHoy + 1;
        }
    }
}
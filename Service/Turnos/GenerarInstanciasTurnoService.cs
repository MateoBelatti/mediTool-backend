using Biblioteca.Entities;
using Repository.Turnos;
using Repository.TurnosFijos;
using Utils.Helpers;

namespace Service.Turnos
{
    public class GenerarInstanciasTurnoService : IGenerarInstanciasTurnoService
    {
        private readonly ITurnoFijoRepository _turnoFijoRepository;
        private readonly ITurnoRepository _turnoRepository;

        public GenerarInstanciasTurnoService(
            ITurnoFijoRepository turnoFijoRepository,
            ITurnoRepository turnoRepository)
        {
            _turnoFijoRepository = turnoFijoRepository;
            _turnoRepository = turnoRepository;
        }

        public async Task GenerarInstancias(TurnoFijo regla, DateTime hastaFecha)
        {
            var ultimo = await _turnoRepository.ObtenerUltimoPorTurnoFijo(regla.Id);
            var fechaActual = ultimo != null
                ? DateOnly.FromDateTime(DateTimeHelper.UtcALocal(ultimo.FechaHora)).AddDays(7)
                : regla.FechaInicio;
            var limite = regla.FechaFin.HasValue && regla.FechaFin.Value < DateOnly.FromDateTime(hastaFecha)
                ? regla.FechaFin.Value.ToDateTime(TimeOnly.MinValue)
                : hastaFecha.Date;

            // Avanzar fechaActual hasta que coincida con el dia de la semana
            while ((int)fechaActual.DayOfWeek != regla.DiaSemana)
            {
                fechaActual = fechaActual.AddDays(1);
            }

            // Saltar las ocurrencias que ya quedaron en el pasado
            var ahora = DateTime.UtcNow;
            while (DateTimeHelper.LocalAUtc(fechaActual, regla.Hora) < ahora)
            {
                fechaActual = fechaActual.AddDays(7);
            }

            var turnosGenerados = new List<Turno>();
            while (fechaActual.ToDateTime(TimeOnly.MinValue) <= limite)
            {
                var fechaHora = DateTimeHelper.LocalAUtc(fechaActual, regla.Hora);
                
                var turno = new Turno
                {
                    TurnoFijoId = regla.Id,
                    PacienteId = regla.PacienteId,
                    ProfesionalId = regla.ProfesionalId,
                    FechaHora = fechaHora,
                    DuracionMin = regla.DuracionMin,
                    Estado = "Pendiente",
                    FechaRegistro = DateTime.UtcNow
                };

                turnosGenerados.Add(turno);
                fechaActual = fechaActual.AddDays(7);
            }

            foreach (var t in turnosGenerados)
            {
                await _turnoRepository.Agregar(t);
            }
            await _turnoRepository.GuardarCambios();
        }

        public async Task GenerarParaTodasLasReglasActivas(DateTime hastaFecha)
        {
            var activas = await _turnoFijoRepository.ObtenerActivos();
            foreach (var regla in activas)
            {
                await GenerarInstancias(regla, hastaFecha);
            }
        }

        public async Task ActualizarInstanciasFuturas(TurnoFijo reglaActualizada)
        {
            var ahora = DateTime.UtcNow;
            var futuros = await _turnoRepository.ObtenerFuturosPorTurnoFijo(reglaActualizada.Id, ahora);

            foreach (var turno in futuros)
            {
                if (turno.Estado == "Cancelado") continue;

                var dateOnly = DateOnly.FromDateTime(DateTimeHelper.UtcALocal(turno.FechaHora));
                
                // Si cambiaron de dia de la semana, movemos la fecha
                if ((int)dateOnly.DayOfWeek != reglaActualizada.DiaSemana)
                {
                    int diff = reglaActualizada.DiaSemana - (int)dateOnly.DayOfWeek;
                    if (diff < -3) diff += 7; // Ajuste menor para mantener cercania (ej de sabado a lunes)
                    dateOnly = dateOnly.AddDays(diff);
                }

                var nuevaFechaHora = DateTimeHelper.LocalAUtc(dateOnly, reglaActualizada.Hora);
                if (nuevaFechaHora < ahora)
                {
                    turno.Estado = "Cancelado";
                    continue;
                }

                turno.FechaHora = nuevaFechaHora;
                turno.DuracionMin = reglaActualizada.DuracionMin;
            }

            await _turnoRepository.ActualizarRango(futuros);
            await _turnoRepository.GuardarCambios();
        }
        public async Task CancelarInstanciasFuturas(int turnoFijoId)
        {
            var ahora = DateTime.UtcNow;
            var futuros = await _turnoRepository.ObtenerFuturosPorTurnoFijo(turnoFijoId, ahora);

            foreach (var turno in futuros)
            {
                turno.Estado = "Cancelado";
            }

            await _turnoRepository.ActualizarRango(futuros);
            await _turnoRepository.GuardarCambios();
        }
    }
}

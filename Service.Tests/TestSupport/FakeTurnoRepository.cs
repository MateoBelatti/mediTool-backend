using Biblioteca.Entities;
using Repository.Turnos;

namespace Service.Tests.TestSupport;

/// <summary>
/// Repositorio de turnos en memoria que replica la semántica relevante del
/// repositorio real (solapamiento y búsqueda del último turno de una regla).
/// Permite verificar de forma end-to-end que la generación de instancias no duplica
/// turnos ya materializados.
/// </summary>
internal sealed class FakeTurnoRepository : ITurnoRepository
{
    private readonly List<Turno> _turnos = new();

    public IReadOnlyList<Turno> Turnos => _turnos;

    public Task<Turno?> ObtenerUltimoPorTurnoFijo(int turnoFijoId) =>
        Task.FromResult(_turnos
            .Where(t => t.TurnoFijoId == turnoFijoId)
            .OrderByDescending(t => t.FechaHora)
            .FirstOrDefault());

    public Task<bool> ExisteSolapamiento(int profesionalId, DateTime fechaHora, int duracionMin, int? excluirTurnoId = null)
    {
        var finNuevo = fechaHora.AddMinutes(duracionMin);
        var solapa = _turnos.Any(t =>
            t.ProfesionalId == profesionalId &&
            (!excluirTurnoId.HasValue || t.Id != excluirTurnoId.Value) &&
            t.Estado != "Cancelado" &&
            t.FechaHora < finNuevo &&
            t.FechaHora.AddMinutes(t.DuracionMin) > fechaHora);

        return Task.FromResult(solapa);
    }

    public Task Agregar(Turno turno)
    {
        if (turno.Id == 0)
        {
            turno.Id = _turnos.Count + 1;
        }

        _turnos.Add(turno);
        return Task.CompletedTask;
    }

    public Task Actualizar(Turno turno) => Task.CompletedTask;

    public Task ActualizarRango(List<Turno> turnos) => Task.CompletedTask;

    public Task GuardarCambios() => Task.CompletedTask;

    public Task<Turno?> ObtenerPorId(int id) =>
        Task.FromResult(_turnos.FirstOrDefault(t => t.Id == id));

    public Task<List<Turno>> ObtenerFuturosPorTurnoFijo(int turnoFijoId, DateTime desde) =>
        Task.FromResult(_turnos.Where(t => t.TurnoFijoId == turnoFijoId && t.FechaHora >= desde).ToList());

    public Task<List<Turno>> ObtenerTodosPorTurnoFijo(int turnoFijoId) =>
        Task.FromResult(_turnos.Where(t => t.TurnoFijoId == turnoFijoId).ToList());

    public Task<(IEnumerable<Turno> Items, int TotalItems)> ObtenerPorRangoFecha(
        DateTime desde, DateTime hasta, int page, int pageSize, int? profesionalId = null) =>
        throw new NotSupportedException("No es necesario para este doble de test.");

    public Task<List<Turno>> ObtenerFacturables(int pacienteId, DateTime desde, DateTime hasta) =>
        throw new NotSupportedException("No es necesario para este doble de test.");
}

using Biblioteca.Entities;
using Microsoft.EntityFrameworkCore;
using Utils.Exceptions;

namespace Biblioteca.Repository
{
    public static class ApplicationDbContextExtensions
    {
        public const string MensajeNoVinculado = "El paciente no está vinculado al profesional.";

        public static Task<bool> EsVinculadoAsync(this ApplicationDbContext context, int pacienteId, int profesionalId, CancellationToken cancellationToken = default)
        {
            return context.PacienteProfesionales
                .AnyAsync(pp => pp.PacienteId == pacienteId && pp.ProfesionalId == profesionalId, cancellationToken);
        }

        public static async Task ValidarVinculacionAsync(this ApplicationDbContext context, int? pacienteId, int profesionalId, CancellationToken cancellationToken = default)
        {
            if (!pacienteId.HasValue)
                return;

            if (!await context.EsVinculadoAsync(pacienteId.Value, profesionalId, cancellationToken))
                throw new ConflictError(MensajeNoVinculado);
        }

        public static async Task ValidarPacienteActivoAsync(this ApplicationDbContext context, int? pacienteId, CancellationToken cancellationToken = default)
        {
            if (!pacienteId.HasValue)
                return;

            var pacienteActivo = await context.Pacientes
                .AnyAsync(p => p.Id == pacienteId.Value && p.Activo, cancellationToken);
            if (!pacienteActivo)
                throw new NotFoundError($"Paciente {pacienteId.Value} no encontrado o está dado de baja.");
        }

        public static async Task VincularAsync(this ApplicationDbContext context, int pacienteId, int profesionalId, CancellationToken cancellationToken = default)
        {
            if (pacienteId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pacienteId), "El id debe ser mayor que 0.");
            if (profesionalId <= 0)
                throw new ArgumentOutOfRangeException(nameof(profesionalId), "El id debe ser mayor que 0.");

            await context.ValidarPacienteActivoAsync(pacienteId, cancellationToken);
            if (await context.EsVinculadoAsync(pacienteId, profesionalId, cancellationToken))
                return;

            context.PacienteProfesionales.Add(new PacienteProfesional
            {
                PacienteId = pacienteId,
                ProfesionalId = profesionalId,
                FechaVinculacion = DateTime.UtcNow
            });
        }
    }
}

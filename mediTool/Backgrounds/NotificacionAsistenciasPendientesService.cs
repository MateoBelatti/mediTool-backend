using Biblioteca.Entities;
using Repository.Notificaciones;
using Repository.Turnos;
using Utils.Enums;

namespace mediTool.Backgrounds
{
    public class NotificacionAsistenciasPendientesService : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificacionAsistenciasPendientesService> _logger;

        public NotificacionAsistenciasPendientesService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificacionAsistenciasPendientesService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RevisarYCrearNotificaciones();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al generar notificaciones de asistencias pendientes.");
                }

                try
                {
                    await Task.Delay(Intervalo, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RevisarYCrearNotificaciones()
        {
            using var scope = _scopeFactory.CreateScope();
            var turnoRepository = scope.ServiceProvider.GetRequiredService<ITurnoRepository>();
            var notificacionRepository = scope.ServiceProvider.GetRequiredService<INotificacionRepository>();

            var pendientes = await turnoRepository.ContarPendientesPorProfesional(DateTime.UtcNow);

            foreach (var (profesionalId, cantidad) in pendientes)
            {
                if (await notificacionRepository.ExisteNotificacionHoy(profesionalId, TipoNotificacion.TurnosSinAsistencia))
                    continue;

                await notificacionRepository.Crear(new Notificacion
                {
                    ProfesionalId = profesionalId,
                    Tipo = TipoNotificacion.TurnosSinAsistencia,
                    Titulo = "Turnos pendientes de asistencia",
                    Mensaje = $"Tenés {cantidad} turnos pendientes de registrar asistencia."
                });
                await notificacionRepository.GuardarCambios();
            }

            _logger.LogInformation(
                "Notificaciones de asistencias pendientes procesadas para {CantidadProfesionales} profesional(es).",
                pendientes.Count);
        }
    }
}

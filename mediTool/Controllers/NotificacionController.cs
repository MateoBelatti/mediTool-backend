using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Notificaciones;
using Utils.DTOs.Notificacion;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionController : ControllerBase
    {
        private readonly INotificacionService _notificacionService;

        public NotificacionController(INotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMias(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? soloNoLeidas = null,
            [FromQuery] int? profesionalId = null)
        {
            var targetProfesionalId = User.GetUserId();
            if (profesionalId.HasValue)
                User.EnsureOwnership(profesionalId.Value);
            else
                profesionalId = targetProfesionalId;

            var result = await _notificacionService.ObtenerPorProfesionalAsync(profesionalId.Value, page, pageSize, soloNoLeidas);
            return Ok(result);
        }

        [HttpGet("no-leidas/count")]
        public async Task<IActionResult> ContarNoLeidas()
        {
            var cantidad = await _notificacionService.ContarNoLeidasAsync(User.GetUserId());
            return Ok(cantidad);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CrearNotificacionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _notificacionService.CrearAsync(dto);
            return Ok(result);
        }

        [HttpPatch("{id}/leer")]
        public async Task<IActionResult> MarcarComoLeida(int id)
        {
            await _notificacionService.MarcarComoLeidaAsync(id, User.GetUserId());
            return NoContent();
        }

        [HttpPatch("leer-todas")]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            var cantidad = await _notificacionService.MarcarTodasLeidasAsync(User.GetUserId());
            return Ok(cantidad);
        }
    }
}

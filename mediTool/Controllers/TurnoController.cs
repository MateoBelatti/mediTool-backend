using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Turnos;
using Utils.DTOs.Turno;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TurnoController : ControllerBase
    {
        private readonly ITurnoService _turnoService;
        private readonly IGenerarInstanciasTurnoService _generarInstanciasService;

        public TurnoController(ITurnoService turnoService, IGenerarInstanciasTurnoService generarInstanciasService)
        {
            _turnoService = turnoService;
            _generarInstanciasService = generarInstanciasService;
        }

        [HttpGet("agenda")]
        public async Task<IActionResult> GetAgenda([FromQuery] DateTime desde, [FromQuery] DateTime hasta, [FromQuery] int? profesionalId)
        {
            if (profesionalId.HasValue)
                User.EnsureOwnership(profesionalId.Value);
            else if (!User.IsAdmin())
                profesionalId = User.GetUserId();

            var turnos = await _turnoService.ObtenerAgenda(desde, hasta, profesionalId);
            return Ok(turnos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var turno = await _turnoService.ObtenerPorId(id);
            User.EnsureOwnership(turno.ProfesionalId);
            return Ok(turno);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSuelto([FromBody] CrearTurnoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!User.IsAdmin())
                dto.ProfesionalId = User.GetUserId();

            var result = await _turnoService.CrearSuelto(dto);
            return Ok(result);
        }

        [HttpPatch("{id}/reprogramar")]
        public async Task<IActionResult> Reprogramar(int id, [FromQuery] DateTime nuevaFechaHora)
        {
            var turno = await _turnoService.ObtenerPorId(id);
            User.EnsureOwnership(turno.ProfesionalId);
            await _turnoService.Reprogramar(id, nuevaFechaHora);
            return NoContent();
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] EstadoTurno nuevoEstado)
        {
            var turno = await _turnoService.ObtenerPorId(id);
            User.EnsureOwnership(turno.ProfesionalId);
            await _turnoService.CambiarEstado(id, nuevoEstado);
            return NoContent();
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("generar-masivo")]
        public async Task<IActionResult> GenerarMasivo([FromQuery] DateTime hastaFecha)
        {
            await _generarInstanciasService.GenerarParaTodasLasReglasActivas(hastaFecha);
            return NoContent();
        }
    }
}

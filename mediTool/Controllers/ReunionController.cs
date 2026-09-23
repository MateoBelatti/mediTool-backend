using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Reuniones;
using Utils.DTOs.Reunion;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReunionController : ControllerBase
    {
        private readonly IReunionService _reunionService;

        public ReunionController(IReunionService reunionService)
        {
            _reunionService = reunionService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reuniones = await _reunionService.GetAllAsync();
            return Ok(reuniones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reunion = await _reunionService.GetByIdAsync(id);
            if (reunion == null)
            {
                return NotFound();
            }

            User.EnsureOwnership(reunion.ProfesionalId);
            return Ok(reunion);
        }

        [HttpGet("profesional/{profesionalId}")]
        public async Task<IActionResult> GetByProfesionalId(int profesionalId)
        {
            User.EnsureOwnership(profesionalId);
            var reuniones = await _reunionService.GetByProfesionalIdAsync(profesionalId);
            return Ok(reuniones);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReunionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!User.IsAdmin())
                dto.ProfesionalId = User.GetUserId();

            var result = await _reunionService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReunionUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _reunionService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            User.EnsureOwnership(existing.ProfesionalId);

            if (!User.IsAdmin())
                dto.ProfesionalId = User.GetUserId();

            var result = await _reunionService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _reunionService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            User.EnsureOwnership(existing.ProfesionalId);

            var success = await _reunionService.DeleteAsync(id);
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.Informes;
using Utils.DTOs.Informe;
using Utils.Helpers;

namespace mediTool.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InformeController : ControllerBase
    {
        private readonly IInformeService _informeService;

        public InformeController(IInformeService informeService)
        {
            _informeService = informeService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var informes = await _informeService.GetAllAsync();
            return Ok(informes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var informe = await _informeService.GetByIdAsync(id);
            if (informe == null)
            {
                return NotFound();
            }

            User.EnsureOwnership(informe.ProfesionalId);
            return Ok(informe);
        }

        [HttpGet("profesional/{profesionalId}")]
        public async Task<IActionResult> GetByProfesionalId(int profesionalId)
        {
            User.EnsureOwnership(profesionalId);
            var informes = await _informeService.GetByProfesionalIdAsync(profesionalId);
            return Ok(informes);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InformeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!User.IsAdmin())
                dto.ProfesionalId = User.GetUserId();

            var result = await _informeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InformeUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _informeService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            User.EnsureOwnership(existing.ProfesionalId);

            if (!User.IsAdmin())
                dto.ProfesionalId = User.GetUserId();

            var result = await _informeService.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _informeService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            User.EnsureOwnership(existing.ProfesionalId);

            var success = await _informeService.DeleteAsync(id);
            return NoContent();
        }
    }
}

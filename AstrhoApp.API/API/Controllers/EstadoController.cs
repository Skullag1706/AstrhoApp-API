using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadosController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public EstadosController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadoResponseDto>>> GetAll()
        {
            var estados = await _context.Estados
                .Select(e => new EstadoResponseDto { EstadoId = e.EstadoId, Nombre = e.Nombre })
                .ToListAsync();

            return Ok(estados);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstadoResponseDto>> Get(int id)
        {
            var e = await _context.Estados.FindAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Estado no encontrado" });

            return Ok(new EstadoResponseDto { EstadoId = e.EstadoId, Nombre = e.Nombre });
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CrearEstadoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest(new { success = false, message = "Nombre inválido" });

            var existe = await _context.Estados.AnyAsync(x => x.Nombre == dto.Nombre);
            if (existe) return BadRequest(new { success = false, message = "Ya existe un estado con ese nombre" });

            var nuevo = new Estado { Nombre = dto.Nombre };
            _context.Estados.Add(nuevo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = nuevo.EstadoId }, new { success = true, data = nuevo.EstadoId });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ModificarEstadoDto dto)
        {
            var e = await _context.Estados.FindAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Estado no encontrado" });

            if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest(new { success = false, message = "Nombre inválido" });

            var conflict = await _context.Estados.AnyAsync(x => x.EstadoId != id && x.Nombre == dto.Nombre);
            if (conflict) return BadRequest(new { success = false, message = "Otro estado ya tiene ese nombre" });

            e.Nombre = dto.Nombre;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Estado actualizado" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var e = await _context.Estados.FindAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Estado no encontrado" });

            // Evitar borrar si está referenciado por agenda
            var usado = await _context.Agenda.AnyAsync(a => a.EstadoId == id);
            if (usado) return BadRequest(new { success = false, message = "No se puede eliminar un estado que está en uso por citas" });

            _context.Estados.Remove(e);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Estado eliminado" });
        }
    }
}
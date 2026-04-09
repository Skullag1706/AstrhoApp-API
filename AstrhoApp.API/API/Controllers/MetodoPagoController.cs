using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetodoPagoController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public MetodoPagoController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // GET: api/MetodoPago
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetodoPagoDto>>> GetAll()
        {
            var list = await _context.MetodoPagos
                .AsNoTracking()
                .Select(m => new MetodoPagoDto
                {
                    MetodopagoId = m.MetodopagoId,
                    Nombre = m.Nombre
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/MetodoPago/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MetodoPagoDto>> GetById(int id)
        {
            var m = await _context.MetodoPagos
                .AsNoTracking()
                .Where(x => x.MetodopagoId == id)
                .Select(x => new MetodoPagoDto
                {
                    MetodopagoId = x.MetodopagoId,
                    Nombre = x.Nombre
                })
                .FirstOrDefaultAsync();

            if (m is null) return NotFound();

            return Ok(m);
        }

        // POST: api/MetodoPago
        [HttpPost]
        public async Task<ActionResult<MetodoPagoDto>> Create([FromBody] CrearMetodoPagoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new MetodoPago
            {
                Nombre = dto.Nombre
            };

            _context.MetodoPagos.Add(entity);
            await _context.SaveChangesAsync();

            var result = new MetodoPagoDto
            {
                MetodopagoId = entity.MetodopagoId,
                Nombre = entity.Nombre
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.MetodopagoId }, result);
        }

        // PUT: api/MetodoPago/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActualizarMetodoPagoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await _context.MetodoPagos.FindAsync(id);
            if (entity is null) return NotFound();

            entity.Nombre = dto.Nombre;

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/MetodoPago/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.MetodoPagos.FindAsync(id);
            if (entity is null) return NotFound();

            _context.MetodoPagos.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
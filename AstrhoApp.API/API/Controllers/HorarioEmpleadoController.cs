using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HorarioEmpleadoController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public HorarioEmpleadoController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // GET: api/HorarioEmpleado
        [HttpGet]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult<IEnumerable<HorarioEmpleadoResponseDto>>> GetAll()
        {
            var items = await _context.HorarioEmpleados
                .Include(he => he.DocumentoEmpleadoNavigation)
                .Include(he => he.HorarioDia)
                    .ThenInclude(hd => hd.Horario)
                .Select(he => new HorarioEmpleadoResponseDto
                {
                    HorarioEmpleadoId = he.HorarioEmpleadoId,
                    HorarioDiaId = he.HorarioDiaId,
                    HorarioId = he.HorarioDia.HorarioId,
                    HorarioNombre = he.HorarioDia.Horario.Nombre,
                    DocumentoEmpleado = he.DocumentoEmpleado,
                    EmpleadoNombre = he.DocumentoEmpleadoNavigation.Nombre,
                    DiaSemana = he.HorarioDia.DiaSemana,
                    HoraInicio = he.HorarioDia.HoraInicio.ToString("HH:mm"),
                    HoraFin = he.HorarioDia.HoraFin.ToString("HH:mm")
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/HorarioEmpleado/5
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult<HorarioEmpleadoResponseDto>> Get(int id)
        {
            var he = await _context.HorarioEmpleados
                .Include(he => he.DocumentoEmpleadoNavigation)
                .Include(he => he.HorarioDia)
                    .ThenInclude(hd => hd.Horario)
                .FirstOrDefaultAsync(x => x.HorarioEmpleadoId == id);

            if (he == null) return NotFound(new { success = false, message = "Asignación no encontrada" });

            return Ok(new HorarioEmpleadoResponseDto
            {
                HorarioEmpleadoId = he.HorarioEmpleadoId,
                HorarioDiaId = he.HorarioDiaId,
                HorarioId = he.HorarioDia.HorarioId,
                HorarioNombre = he.HorarioDia.Horario.Nombre,
                DocumentoEmpleado = he.DocumentoEmpleado,
                EmpleadoNombre = he.DocumentoEmpleadoNavigation.Nombre,
                DiaSemana = he.HorarioDia.DiaSemana,
                HoraInicio = he.HorarioDia.HoraInicio.ToString("HH:mm"),
                HoraFin = he.HorarioDia.HoraFin.ToString("HH:mm")
            });
        }

        // GET: api/HorarioEmpleado/empleado/{documentoEmpleado}
        [HttpGet("empleado/{documentoEmpleado}")]
        public async Task<ActionResult<IEnumerable<HorarioEmpleadoResponseDto>>> GetByEmpleado(string documentoEmpleado)
        {
            var asignaciones = await _context.HorarioEmpleados
                .Where(x => x.DocumentoEmpleado == documentoEmpleado)
                .Include(he => he.DocumentoEmpleadoNavigation)
                .Include(he => he.HorarioDia)
                    .ThenInclude(hd => hd.Horario)
                .Select(he => new HorarioEmpleadoResponseDto
                {
                    HorarioEmpleadoId = he.HorarioEmpleadoId,
                    HorarioDiaId = he.HorarioDiaId,
                    HorarioId = he.HorarioDia.HorarioId,
                    HorarioNombre = he.HorarioDia.Horario.Nombre,
                    DocumentoEmpleado = he.DocumentoEmpleado,
                    EmpleadoNombre = he.DocumentoEmpleadoNavigation.Nombre,
                    DiaSemana = he.HorarioDia.DiaSemana,
                    HoraInicio = he.HorarioDia.HoraInicio.ToString("HH:mm"),
                    HoraFin = he.HorarioDia.HoraFin.ToString("HH:mm")
                })
                .ToListAsync();

            return Ok(asignaciones);
        }

        // POST: api/HorarioEmpleado
        [HttpPost]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> Create([FromBody] CrearHorarioEmpleadoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var empleadoExiste = await _context.Empleados.AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);
            if (!empleadoExiste) return BadRequest(new { success = false, message = "Empleado no encontrado" });

            var diaExiste = await _context.HorarioDias.AnyAsync(h => h.HorarioDiaId == dto.HorarioDiaId);
            if (!diaExiste) return BadRequest(new { success = false, message = "Día de horario no encontrado" });

            var yaAsignado = await _context.HorarioEmpleados.AnyAsync(x => x.HorarioDiaId == dto.HorarioDiaId && x.DocumentoEmpleado == dto.DocumentoEmpleado);
            if (yaAsignado) return BadRequest(new { success = false, message = "Empleado ya asignado a este día" });

            var nuevo = new HorarioEmpleado
            {
                HorarioDiaId = dto.HorarioDiaId,
                DocumentoEmpleado = dto.DocumentoEmpleado
            };

            _context.HorarioEmpleados.Add(nuevo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = nuevo.HorarioEmpleadoId }, new { success = true, data = nuevo.HorarioEmpleadoId });
        }

        // POST: api/HorarioEmpleado/masivo
        [HttpPost("masivo")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> CreateMasivo([FromBody] AsignacionMasivaDto dto)
        {
            if (dto?.Dias == null || !dto.Dias.Any()) return BadRequest(new { success = false, message = "Datos inválidos" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int creados = 0;
                var errores = new List<string>();

                foreach (var dia in dto.Dias)
                {
                    var diaExiste = await _context.HorarioDias.AnyAsync(h => h.HorarioDiaId == dia.HorarioDiaId);
                    if (!diaExiste)
                    {
                        errores.Add($"El día con ID {dia.HorarioDiaId} no existe.");
                        continue;
                    }

                    foreach (var element in dia.Empleados)
                    {
                        string? doc = null;

                        if (element.ValueKind == JsonValueKind.String)
                        {
                            doc = element.GetString();
                        }
                        else if (element.ValueKind == JsonValueKind.Object)
                        {
                            if (element.TryGetProperty("documentoEmpleado", out var prop) ||
                                element.TryGetProperty("DocumentoEmpleado", out prop))
                            {
                                doc = prop.GetString();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(doc))
                        {
                            errores.Add("Se encontró un formato de empleado inválido o vacío.");
                            continue;
                        }

                        // Normalizar documento (quitar espacios)
                        doc = doc.Trim();

                        var empleadoExiste = await _context.Empleados.AnyAsync(e => e.DocumentoEmpleado == doc);
                        if (!empleadoExiste)
                        {
                            errores.Add($"El empleado con documento {doc} no existe.");
                            continue;
                        }

                        var yaAsignado = await _context.HorarioEmpleados.AnyAsync(x => x.HorarioDiaId == dia.HorarioDiaId && x.DocumentoEmpleado == doc);
                        if (yaAsignado)
                        {
                            // No es un error crítico, pero es bueno saberlo
                            continue;
                        }

                        _context.HorarioEmpleados.Add(new HorarioEmpleado
                        {
                            HorarioDiaId = dia.HorarioDiaId,
                            DocumentoEmpleado = doc
                        });
                        creados++;
                    }
                }

                if (creados > 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                return Ok(new 
                { 
                    success = true, 
                    message = $"{creados} asignaciones creadas",
                    errors = errores.Any() ? errores : null 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // PUT: api/HorarioEmpleado/5
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> Update(int id, [FromBody] ModificarHorarioEmpleadoDto dto)
        {
            var asignacion = await _context.HorarioEmpleados.FindAsync(id);
            if (asignacion == null) return NotFound(new { success = false, message = "Asignación no encontrada" });

            if (dto.HorarioDiaId.HasValue)
            {
                var diaExiste = await _context.HorarioDias.AnyAsync(h => h.HorarioDiaId == dto.HorarioDiaId.Value);
                if (!diaExiste) return BadRequest(new { success = false, message = "Día no encontrado" });
                asignacion.HorarioDiaId = dto.HorarioDiaId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.DocumentoEmpleado))
            {
                var empleadoExiste = await _context.Empleados.AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);
                if (!empleadoExiste) return BadRequest(new { success = false, message = "Empleado no encontrado" });
                asignacion.DocumentoEmpleado = dto.DocumentoEmpleado;
            }

            // Validar duplicado
            var duplicado = await _context.HorarioEmpleados.AnyAsync(x => 
                x.HorarioDiaId == asignacion.HorarioDiaId && 
                x.DocumentoEmpleado == asignacion.DocumentoEmpleado && 
                x.HorarioEmpleadoId != id);

            if (duplicado) return BadRequest(new { success = false, message = "Ya existe esta asignación" });

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Asignación actualizada" });
        }

        // DELETE: api/HorarioEmpleado/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> Delete(int id)
        {
            var he = await _context.HorarioEmpleados.FindAsync(id);
            if (he == null) return NotFound(new { success = false, message = "Asignación no encontrada" });

            _context.HorarioEmpleados.Remove(he);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Asignación eliminada" });
        }
    }
}

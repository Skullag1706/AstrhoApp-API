using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HorarioController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public HorarioController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        private bool TryParseHora(string input, out TimeOnly hora)
        {
            hora = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var formats = new[] { "HH:mm", "H:mm", "HH:mm:ss", "H:mm:ss" };
            return TimeOnly.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out hora);
        }

        // GET: api/Horario
        [HttpGet]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Horarios
                    .Include(h => h.HorarioDias)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(h => h.Nombre.ToLower().Contains(busqueda));
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var horarios = await query
                    .OrderBy(h => h.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(h => new HorarioResponseDto
                    {
                        HorarioId = h.HorarioId,
                        Nombre = h.Nombre,
                        Estado = h.Estado,
                        Dias = h.HorarioDias.Select(d => new HorarioDiaDto
                        {
                            HorarioDiaId = d.HorarioDiaId,
                            DiaSemana = d.DiaSemana,
                            HoraInicio = d.HoraInicio,
                            HoraFin = d.HoraFin
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    totalRegistros,
                    totalPaginas,
                    paginaActual = pagina,
                    data = horarios
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: api/Horario/5
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult<HorarioResponseDto>> Get(int id)
        {
            var h = await _context.Horarios
                .Include(h => h.HorarioDias)
                .FirstOrDefaultAsync(x => x.HorarioId == id);

            if (h == null) return NotFound(new { success = false, message = "Horario no encontrado" });

            return Ok(new HorarioResponseDto
            {
                HorarioId = h.HorarioId,
                Nombre = h.Nombre,
                Estado = h.Estado,
                Dias = h.HorarioDias.Select(d => new HorarioDiaDto
                {
                    HorarioDiaId = d.HorarioDiaId,
                    DiaSemana = d.DiaSemana,
                    HoraInicio = d.HoraInicio,
                    HoraFin = d.HoraFin
                }).ToList()
            });
        }

        // POST: api/Horario
        [HttpPost]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> Create([FromBody] CrearHorarioDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevoHorario = new Horario
                {
                    Nombre = dto.Nombre,
                    Estado = dto.Estado
                };

                _context.Horarios.Add(nuevoHorario);
                await _context.SaveChangesAsync(); // Para obtener el ID

                foreach (var diaDto in dto.Dias)
                {
                    if (!TryParseHora(diaDto.HoraInicio, out var start) || !TryParseHora(diaDto.HoraFin, out var end))
                    {
                        return BadRequest(new { success = false, message = $"Formato de hora inválido para el día {diaDto.DiaSemana}" });
                    }

                    nuevoHorario.HorarioDias.Add(new HorarioDia
                    {
                        DiaSemana = diaDto.DiaSemana,
                        HoraInicio = start,
                        HoraFin = end
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new HorarioResponseDto
                {
                    HorarioId = nuevoHorario.HorarioId,
                    Nombre = nuevoHorario.Nombre,
                    Estado = nuevoHorario.Estado,
                    Dias = nuevoHorario.HorarioDias.Select(d => new HorarioDiaDto
                    {
                        HorarioDiaId = d.HorarioDiaId,
                        DiaSemana = d.DiaSemana,
                        HoraInicio = d.HoraInicio,
                        HoraFin = d.HoraFin
                    }).ToList()
                };

                return CreatedAtAction(nameof(Get), new { id = nuevoHorario.HorarioId }, new { success = true, data = response });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // PUT: api/Horario/5
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> Update(int id, [FromBody] ModificarHorarioDto dto)
        {
            var h = await _context.Horarios
                .Include(h => h.HorarioDias)
                .FirstOrDefaultAsync(x => x.HorarioId == id);

            if (h == null) return NotFound(new { success = false, message = "Horario no encontrado" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Nombre)) h.Nombre = dto.Nombre;
                if (dto.Estado.HasValue) h.Estado = dto.Estado.Value;

                if (dto.Dias != null)
                {
                    // 1. Obtener los IDs que vienen del DTO (los que queremos mantener o actualizar)
                    var idsEnviados = dto.Dias
                        .Where(d => d.HorarioDiaId.HasValue && d.HorarioDiaId > 0)
                        .Select(d => d.HorarioDiaId!.Value)
                        .ToList();

                    // 2. Identificar y eliminar los que ya no están en la lista enviada (sobran)
                    var diasActuales = h.HorarioDias.ToList();
                    var diasAEliminar = diasActuales
                        .Where(d => !idsEnviados.Contains(d.HorarioDiaId))
                        .ToList();
                    
                    if (diasAEliminar.Any())
                    {
                        _context.HorarioDias.RemoveRange(diasAEliminar);
                    }

                    // 3. Procesar el merge (actualizar existentes o crear nuevos)
                    foreach (var diaDto in dto.Dias)
                    {
                        if (!TryParseHora(diaDto.HoraInicio, out var start) || !TryParseHora(diaDto.HoraFin, out var end))
                        {
                            return BadRequest(new { success = false, message = $"Formato de hora inválido para el día {diaDto.DiaSemana}" });
                        }

                        if (diaDto.HorarioDiaId.HasValue && diaDto.HorarioDiaId > 0)
                        {
                            // Si el día existe → lo actualizamos SOLO si hay cambios para evitar marcas innecesarias
                            var diaExistente = h.HorarioDias.FirstOrDefault(d => d.HorarioDiaId == diaDto.HorarioDiaId.Value);
                            if (diaExistente != null)
                            {
                                // Solo actualizamos si los valores son diferentes
                                if (diaExistente.DiaSemana != diaDto.DiaSemana || 
                                    diaExistente.HoraInicio != start || 
                                    diaExistente.HoraFin != end)
                                {
                                    diaExistente.DiaSemana = diaDto.DiaSemana;
                                    diaExistente.HoraInicio = start;
                                    diaExistente.HoraFin = end;
                                    _context.Entry(diaExistente).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                // Si el ID no pertenece a este horario, lo ignoramos o lo creamos como nuevo sin ese ID
                                h.HorarioDias.Add(new HorarioDia
                                {
                                    DiaSemana = diaDto.DiaSemana,
                                    HoraInicio = start,
                                    HoraFin = end
                                });
                            }
                        }
                        else
                        {
                            // Si no tiene ID → lo creamos
                            h.HorarioDias.Add(new HorarioDia
                            {
                                DiaSemana = diaDto.DiaSemana,
                                HoraInicio = start,
                                HoraFin = end
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new HorarioResponseDto
                {
                    HorarioId = h.HorarioId,
                    Nombre = h.Nombre,
                    Estado = h.Estado,
                    Dias = h.HorarioDias.Select(d => new HorarioDiaDto
                    {
                        HorarioDiaId = d.HorarioDiaId,
                        DiaSemana = d.DiaSemana,
                        HoraInicio = d.HoraInicio,
                        HoraFin = d.HoraFin
                    }).ToList()
                };

                return Ok(new { success = true, message = "Horario actualizado", data = response });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // PATCH: api/Horario/5/toggle
        [HttpPatch("{id}/toggle")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> ToggleEstado(int id)
        {
            var h = await _context.Horarios.FindAsync(id);
            if (h == null) return NotFound(new { success = false, message = "Horario no encontrado" });

            h.Estado = !h.Estado;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, estado = h.Estado });
        }

        // DELETE: api/Horario/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Horarios")]
        public async Task<ActionResult> Delete(int id)
        {
            var h = await _context.Horarios
                .Include(h => h.HorarioDias)
                .FirstOrDefaultAsync(x => x.HorarioId == id);

            if (h == null) return NotFound(new { success = false, message = "Horario no encontrado" });

            try
            {
                _context.Horarios.Remove(h);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Horario eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"No se pudo eliminar el horario: {ex.Message}" });
            }
        }
    }
}

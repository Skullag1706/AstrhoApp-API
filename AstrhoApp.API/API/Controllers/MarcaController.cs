using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MarcaController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public MarcaController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR TODAS LAS MARCAS
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> ListarMarcas([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Marcas.AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(m => m.Nombre.ToLower().Contains(busqueda));
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var marcas = await query
                    .OrderBy(m => m.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(m => new MarcaListDto
                    {
                        MarcaId = m.MarcaId,
                        Nombre = m.Nombre
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = marcas 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar marcas: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR MARCA POR ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<MarcaResponseDto>> BuscarMarca(int id)
        {
            try
            {
                var marca = await _context.Marcas.FindAsync(id);

                if (marca == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Marca no encontrada"
                    });
                }

                var response = new MarcaResponseDto
                {
                    MarcaId = marca.MarcaId,
                    Nombre = marca.Nombre
                };

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar marca: {ex.Message}"
                });
            }
        }

        // ============================================================
        // CREAR UNA NUEVA MARCA
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearMarca([FromBody] CrearMarcaDto marcaDto)
        {
            try
            {
                if (await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == marcaDto.Nombre.ToLower()))
                {
                    return BadRequest(new { success = false, message = "Ya existe una marca con este nombre" });
                }

                var marca = new Marca
                {
                    Nombre = marcaDto.Nombre
                };

                _context.Marcas.Add(marca);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Marca creada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al crear marca: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR UNA MARCA
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarMarca(int id, [FromBody] ActualizarMarcaDto marcaDto)
        {
            try
            {
                var marca = await _context.Marcas.FindAsync(id);

                if (marca == null)
                {
                    return NotFound(new { success = false, message = "Marca no encontrada" });
                }

                if (await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == marcaDto.Nombre.ToLower() && m.MarcaId != id))
                {
                    return BadRequest(new { success = false, message = "Ya existe otra marca con este nombre" });
                }

                marca.Nombre = marcaDto.Nombre;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Marca actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar marca: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ELIMINAR UNA MARCA
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarMarca(int id)
        {
            try
            {
                var marca = await _context.Marcas.FindAsync(id);

                if (marca == null)
                {
                    return NotFound(new { success = false, message = "Marca no encontrada" });
                }

                // Verificar si tiene insumos asociados antes de eliminar (opcional, pero recomendado)
                // En el DbContext vi que Insumo tiene una relación con Categoria, pero no vi Marca en Insumo.
                // Revisemos el modelo Insumo.cs por si acaso.
                
                _context.Marcas.Remove(marca);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Marca eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar marca: {ex.Message}"
                });
            }
        }
    }
}

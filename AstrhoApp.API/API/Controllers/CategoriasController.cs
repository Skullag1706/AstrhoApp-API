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
    public class CategoriasController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public CategoriasController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR TODAS LAS CATEGORÍAS
        // ============================================================
        [HttpGet]
        [Authorize(Policy = "perm:Categoria")]
        public async Task<ActionResult> ListarCategorias([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Categoria.AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(c => 
                        c.Nombre.ToLower().Contains(busqueda) || 
                        (c.Descripcion != null && c.Descripcion.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var categorias = await query
                    .OrderBy(c => c.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(c => new CategoriaListDto
                    {
                        CategoriaId = c.CategoriaId,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        Estado = c.Estado,
                        CantidadProductos = c.Insumos.Count()
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = categorias 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar categorías: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR CATEGORÍA POR ID
        // ============================================================
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Categoria")]
        public async Task<ActionResult<CategoriaResponseDto>> BuscarCategoria(int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);

                if (categoria == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Categoría no encontrada"
                    });
                }

                var response = new CategoriaResponseDto
                {
                    CategoriaId = categoria.CategoriaId,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Estado = categoria.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar categoría: {ex.Message}"
                });
            }
        }

        // ============================================================
        // REGISTRAR NUEVA CATEGORÍA
        // ============================================================
        [HttpPost]
        [Authorize(Policy = "perm:Categoria")]
        public async Task<ActionResult<CategoriaResponseDto>> RegistrarCategoria([FromBody] CrearCategoriaDto dto)
        {
            try
            {
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre de la categoría es obligatorio"
                    });
                }

                // Verificar que el nombre de la categoría no exista
                var categoriaExiste = await _context.Categoria
                    .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower());

                if (categoriaExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Ya existe una categoría con ese nombre"
                    });
                }

                // Crear nueva categoría
                var nuevaCategoria = new Categorium
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Estado = true
                };

                _context.Categoria.Add(nuevaCategoria);
                await _context.SaveChangesAsync();

                var response = new CategoriaResponseDto
                {
                    CategoriaId = nuevaCategoria.CategoriaId,
                    Nombre = nuevaCategoria.Nombre,
                    Descripcion = nuevaCategoria.Descripcion,
                    Estado = nuevaCategoria.Estado
                };

                return CreatedAtAction(nameof(BuscarCategoria), new { id = nuevaCategoria.CategoriaId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al registrar categoría: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR CATEGORÍA
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Categoria")]
        public async Task<ActionResult<CategoriaResponseDto>> ActualizarCategoria(int id, [FromBody] ActualizarCategoriaDto dto)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);

                if (categoria == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Categoría no encontrada"
                    });
                }

                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre de la categoría es obligatorio"
                    });
                }

                // Verificar que el nuevo nombre no exista (si cambió)
                if (categoria.Nombre.ToLower() != dto.Nombre.ToLower())
                {
                    var nombreExiste = await _context.Categoria
                        .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower() && c.CategoriaId != id);

                    if (nombreExiste)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Ya existe una categoría con ese nombre"
                        });
                    }
                }

                // Actualizar datos
                categoria.Nombre = dto.Nombre;
                categoria.Descripcion = dto.Descripcion;
                categoria.Estado = dto.Estado;

                await _context.SaveChangesAsync();

                var response = new CategoriaResponseDto
                {
                    CategoriaId = categoria.CategoriaId,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Estado = categoria.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar categoría: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ELIMINAR CATEGORÍA
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Categoria")]
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);

                if (categoria == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Categoría no encontrada"
                    });
                }

                // Verificar si la categoría tiene productos asociados
                var tieneProductos = await _context.Insumos.AnyAsync(p => p.CategoriaId == id);

                if (tieneProductos)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar la categoría porque tiene productos asociados"
                    });
                }

                _context.Categoria.Remove(categoria);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Categoría eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar categoría: {ex.Message}"
                });
            }
        }

        // ============================================================
        // OBTENER PRODUCTOS POR CATEGORÍA
        // ============================================================
        [HttpGet("{id}/productos")]
        [Authorize(Policy = "perm:Categoria")]
        public async Task<ActionResult> ObtenerProductosPorCategoria(int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);

                if (categoria == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Categoría no encontrada"
                    });
                }

                var productos = await _context.Insumos
                    .Where(p => p.CategoriaId == id)
                    .Select(p => new
                    {
                        p.InsumoId,
                        p.Sku,
                        p.Nombre,
                        p.Descripcion,
                        p.Estado,
                    })
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(new
                {
                    categoriaId = id,
                    categoriaNombre = categoria.Nombre,
                    cantidadProductos = productos.Count,
                    productos = productos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener productos: {ex.Message}"
                });
            }
        }
    }
}

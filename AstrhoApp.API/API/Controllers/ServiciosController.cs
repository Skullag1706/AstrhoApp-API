using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public ServiciosController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR TODOS LOS SERVICIOS
        // ============================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ListarServicios([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Servicios.AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(s => 
                        s.Nombre.ToLower().Contains(busqueda) || 
                        (s.Descripcion != null && s.Descripcion.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var servicios = await query
                    .OrderBy(s => s.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(s => new ServicioListDto
                    {
                        ServicioId = s.ServicioId,
                        Nombre = s.Nombre,
                        Precio = s.Precio,
                        Duracion = s.Duracion,
                        Estado = s.Estado,
                        Imagen = s.Imagen
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = servicios 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar servicios: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR SERVICIO POR ID
        // ============================================================
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Servicios")]
        public async Task<ActionResult<ServicioResponseDto>> BuscarServicio(int id)
        {
            try
            {
                var servicio = await _context.Servicios
                    .FirstOrDefaultAsync(s => s.ServicioId == id);

                if (servicio == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Servicio no encontrado"
                    });
                }

                var response = new ServicioResponseDto
                {
                    ServicioId = servicio.ServicioId,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    Precio = servicio.Precio,
                    Duracion = servicio.Duracion,
                    Estado = servicio.Estado,
                    Imagen = servicio.Imagen
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar servicio: {ex.Message}"
                });
            }
        }

        // ============================================================
        // REGISTRAR NUEVO SERVICIO
        // ============================================================
        [HttpPost]
        [Authorize(Policy ="perm:Servicios")]
        public async Task<ActionResult<ServicioResponseDto>> RegistrarServicio([FromForm] CrearServicioDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { success = false, message = "El nombre es obligatorio" });

                if (dto.Precio <= 0)
                    return BadRequest(new { success = false, message = "El precio debe ser mayor a cero" });

                if (dto.Duracion <= 0)
                    return BadRequest(new { success = false, message = "La duración debe ser mayor a cero" });

                var existe = await _context.Servicios
                    .AnyAsync(s => s.Nombre.ToLower() == dto.Nombre.ToLower());

                if (existe)
                    return BadRequest(new { success = false, message = "Ya existe un servicio con ese nombre" });

                string? nombreArchivo = null;

                // ==========================
                // GUARDAR IMAGEN
                // ==========================
                if (dto.Imagen != null)
                {
                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagenes");

                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(dto.Imagen.FileName);

                    var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await dto.Imagen.CopyToAsync(stream);
                    }
                }

                var nuevoServicio = new Servicio
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Precio = dto.Precio,
                    Duracion = dto.Duracion,
                    Estado = true,
                    Imagen = nombreArchivo
                };

                _context.Servicios.Add(nuevoServicio);
                await _context.SaveChangesAsync();

                return Ok(new ServicioResponseDto
                {
                    ServicioId = nuevoServicio.ServicioId,
                    Nombre = nuevoServicio.Nombre,
                    Descripcion = nuevoServicio.Descripcion,
                    Precio = nuevoServicio.Precio,
                    Duracion = nuevoServicio.Duracion,
                    Estado = nuevoServicio.Estado,
                    Imagen = nuevoServicio.Imagen != null
                        ? $"{Request.Scheme}://{Request.Host}/imagenes/{nuevoServicio.Imagen}"
                        : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al registrar servicio: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR SERVICIO
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Servicios")]
        public async Task<ActionResult<ServicioResponseDto>> ActualizarServicio(int id, [FromForm] ActualizarServicioDto dto)
        {
            try
            {
                var servicio = await _context.Servicios
                    .FirstOrDefaultAsync(s => s.ServicioId == id);

                if (servicio == null)
                    return NotFound(new { success = false, message = "Servicio no encontrado" });

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { success = false, message = "El nombre es obligatorio" });

                if (dto.Precio <= 0)
                    return BadRequest(new { success = false, message = "El precio debe ser mayor a cero" });

                if (dto.Duracion <= 0)
                    return BadRequest(new { success = false, message = "La duración debe ser mayor a cero" });

                // Validar nombre duplicado
                if (servicio.Nombre.ToLower() != dto.Nombre.ToLower())
                {
                    var nombreExiste = await _context.Servicios
                        .AnyAsync(s => s.Nombre.ToLower() == dto.Nombre.ToLower() && s.ServicioId != id);

                    if (nombreExiste)
                        return BadRequest(new { success = false, message = "Ya existe un servicio con ese nombre" });
                }

                // ==========================
                // ACTUALIZAR IMAGEN
                // ==========================
                if (dto.Imagen != null)
                {
                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagenes");

                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    // Eliminar imagen anterior
                    if (!string.IsNullOrEmpty(servicio.Imagen))
                    {
                        var rutaAnterior = Path.Combine(carpeta, servicio.Imagen);
                        if (System.IO.File.Exists(rutaAnterior))
                            System.IO.File.Delete(rutaAnterior);
                    }

                    var nuevoNombre = Guid.NewGuid().ToString() + Path.GetExtension(dto.Imagen.FileName);
                    var nuevaRuta = Path.Combine(carpeta, nuevoNombre);

                    using (var stream = new FileStream(nuevaRuta, FileMode.Create))
                    {
                        await dto.Imagen.CopyToAsync(stream);
                    }

                    servicio.Imagen = nuevoNombre;
                }

                servicio.Nombre = dto.Nombre;
                servicio.Descripcion = dto.Descripcion;
                servicio.Precio = dto.Precio;
                servicio.Duracion = dto.Duracion;
                servicio.Estado = dto.Estado;

                await _context.SaveChangesAsync();

                return Ok(new ServicioResponseDto
                {
                    ServicioId = servicio.ServicioId,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    Precio = servicio.Precio,
                    Duracion = servicio.Duracion,
                    Estado = servicio.Estado,
                    Imagen = servicio.Imagen != null
                        ? $"{Request.Scheme}://{Request.Host}/imagenes/{servicio.Imagen}"
                        : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar servicio: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ELIMINAR SERVICIO
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Servicios")]
        public async Task<ActionResult> EliminarServicio(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var servicio = await _context.Servicios
                    .FirstOrDefaultAsync(s => s.ServicioId == id);

                if (servicio == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Servicio no encontrado"
                    });
                }

                // Verificar si el servicio tiene citas asociadas
                var tieneCitas = await _context.ServicioAgenda.AnyAsync(sa => sa.ServicioId == id);

                if (tieneCitas)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar el servicio porque tiene citas asociadas"
                    });
                }

                // Eliminar servicio
                _context.Servicios.Remove(servicio);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Servicio eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar servicio: {ex.Message}"
                });
            }
        }
    }
}
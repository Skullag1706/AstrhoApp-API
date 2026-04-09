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
    public class EmpleadosController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public EmpleadosController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR TODOS LOS EMPLEADOS
        // ============================================================
        [HttpGet]
        [Authorize(Policy = "perm:Empleados")]
        public async Task<ActionResult> ListarEmpleados([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Empleados.AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(e => 
                        e.Nombre.ToLower().Contains(busqueda) || 
                        e.DocumentoEmpleado.ToLower().Contains(busqueda) ||
                        (e.Telefono != null && e.Telefono.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var empleados = await query
                    .OrderBy(e => e.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(e => new EmpleadoResponseDto
                    {
                        DocumentoEmpleado = e.DocumentoEmpleado,
                        TipoDocumento = e.TipoDocumento,
                        UsuarioId = e.UsuarioId,
                        Nombre = e.Nombre,
                        Telefono = e.Telefono,
                        Estado = e.Estado
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = empleados 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar empleados: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR EMPLEADO POR DOCUMENTO
        // ============================================================
        [HttpGet("{documentoEmpleado}")]
        [Authorize(Policy = "perm:Empleados")]
        public async Task<ActionResult<EmpleadoResponseDto>> BuscarEmpleado(string documentoEmpleado)
        {
            try
            {
                var empleado = await _context.Empleados
                    .Include(e => e.Usuario)
                    .FirstOrDefaultAsync(e => e.DocumentoEmpleado == documentoEmpleado);

                if (empleado == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Empleado no encontrado"
                    });
                }

                var response = new EmpleadoResponseDto
                {
                    DocumentoEmpleado = empleado.DocumentoEmpleado,
                    TipoDocumento = empleado.TipoDocumento,
                    UsuarioId = empleado.UsuarioId,
                    Nombre = empleado.Nombre,
                    Telefono = empleado.Telefono,
                    Dirección = empleado.Dirección,
                    Estado = empleado.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar empleado: {ex.Message}"
                });
            }
        }

        // ============================================================
        // REGISTRAR NUEVO EMPLEADO
        // ============================================================
        [HttpPost]
        [Authorize(Policy = "perm:Empleados")]
        public async Task<ActionResult<EmpleadoResponseDto>> RegistrarEmpleado([FromBody] CrearEmpleadoDto dto)
        {
            try
            {
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del empleado es obligatorio"
                    });
                }

                // Validar que el documento no esté vacío
                if (string.IsNullOrWhiteSpace(dto.DocumentoEmpleado))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El documento es obligatorio"
                    });
                }

                // Verificar que el usuario existe
                var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
                if (usuario == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El usuario especificado no existe"
                    });
                }

                // Verificar que el documento no exista
                var empleadoExiste = await _context.Empleados
                    .AnyAsync(e => e.DocumentoEmpleado == dto.DocumentoEmpleado);

                if (empleadoExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Ya existe un empleado con ese documento"
                    });
                }

                // Crear nuevo empleado
                var nuevoEmpleado = new Empleado
                {
                    DocumentoEmpleado = dto.DocumentoEmpleado,
                    UsuarioId = dto.UsuarioId,
                    TipoDocumento = dto.TipoDocumento,
                    Nombre = dto.Nombre,
                    Telefono = dto.Telefono,
                    Dirección = dto.Dirección,
                    Estado = true
                };

                _context.Empleados.Add(nuevoEmpleado);
                await _context.SaveChangesAsync();

                // Cargar el usuario para la respuesta
                await _context.Entry(nuevoEmpleado).Reference(e => e.Usuario).LoadAsync();

                var response = new EmpleadoResponseDto
                {
                    DocumentoEmpleado = nuevoEmpleado.DocumentoEmpleado,
                    UsuarioId = nuevoEmpleado.UsuarioId,
                    TipoDocumento = nuevoEmpleado.TipoDocumento,
                    Nombre = nuevoEmpleado.Nombre,
                    Telefono = nuevoEmpleado.Telefono,
                    Dirección = nuevoEmpleado.Dirección,
                    Estado = nuevoEmpleado.Estado
                };

                return CreatedAtAction(nameof(BuscarEmpleado), new { documentoEmpleado = nuevoEmpleado.DocumentoEmpleado }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al registrar empleado: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR EMPLEADO
        // ============================================================
        [HttpPut("{documentoEmpleado}")]
        [Authorize(Policy = "perm:Empleados")]
        public async Task<ActionResult<EmpleadoResponseDto>> ActualizarEmpleado(string documentoEmpleado, [FromBody] ActualizarEmpleadoDto dto)
        {
            try
            {
                var empleado = await _context.Empleados
                    .Include(e => e.Usuario)
                    .FirstOrDefaultAsync(e => e.DocumentoEmpleado == documentoEmpleado);

                if (empleado == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Empleado no encontrado"
                    });
                }

                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del empleado es obligatorio"
                    });
                }

                // Actualizar datos
                empleado.TipoDocumento = dto.TipoDocumento;
                empleado.Nombre = dto.Nombre;
                empleado.Telefono = dto.Telefono;
                empleado.Dirección = dto.Dirección;
                empleado.Estado = dto.Estado;

                await _context.SaveChangesAsync();

                var response = new EmpleadoResponseDto
                {
                    DocumentoEmpleado = empleado.DocumentoEmpleado,
                    UsuarioId = empleado.UsuarioId,
                    TipoDocumento = empleado.TipoDocumento,
                    Nombre = empleado.Nombre,
                    Telefono = empleado.Telefono,
                    Dirección = empleado.Dirección,
                    Estado = empleado.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar empleado: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ELIMINAR EMPLEADO
        // ============================================================
        [HttpDelete("{documentoEmpleado}")]
        [Authorize(Policy = "perm:Empleados")]
        public async Task<ActionResult> EliminarEmpleado(string documentoEmpleado)
        {
            try
            {
                var empleado = await _context.Empleados.FindAsync(documentoEmpleado);

                if (empleado == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Empleado no encontrado"
                    });
                }

                // Verificar si el empleado tiene citas
                var tieneCitas = await _context.Agenda.AnyAsync(a => a.DocumentoEmpleado == documentoEmpleado);

                // Verificar si el empleado tiene entregas de insumos
                var tieneEntregas = await _context.Entregainsumos.AnyAsync(ei => ei.DocumentoEmpleado == documentoEmpleado);

                // Verificar si tiene horarios asignados
                var tieneHorarios = await _context.HorarioEmpleados.AnyAsync(he => he.DocumentoEmpleado == documentoEmpleado);

                if (tieneCitas || tieneEntregas || tieneHorarios)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar el empleado porque tiene registros asociados (citas, entregas o horarios)"
                    });
                }

                var usuarioId = empleado.UsuarioId;
                var usuario = await _context.Usuarios.FindAsync(usuarioId);

                _context.Empleados.Remove(empleado);
                
                if (usuario != null)
                {
                    _context.Usuarios.Remove(usuario);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Empleado eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar empleado: {ex.Message}"
                });
            }
        }
    }
}
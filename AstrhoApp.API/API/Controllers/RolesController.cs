using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public RolesController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR TODOS LOS ROLES
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> ListarRoles([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Rols
                    .Include(r => r.RolPermisos!)
                        .ThenInclude(rp => rp.Permiso)
                    .AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(r => 
                        r.Nombre.ToLower().Contains(busqueda) || 
                        (r.Descripcion != null && r.Descripcion.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var roles = await query
                    .OrderBy(r => r.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(r => new RolListDto
                    {
                        RolId = r.RolId,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Permisos = r.RolPermisos!
                                      .Where(rp => rp.Permiso != null)
                                      .Select(rp => rp.Permiso!.Nombre)
                                      .OrderBy(n => n)
                                      .ToList(),
                        Estado = r.Estado
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = roles 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar roles: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR ROL POR ID (incluye permisos para edición)
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<RolResponseDto>> BuscarRol(int id)
        {
            try
            {
                var rol = await _context.Rols
                    .Include(r => r.RolPermisos!)
                        .ThenInclude(rp => rp.Permiso)
                    .FirstOrDefaultAsync(r => r.RolId == id);

                if (rol == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rol no encontrado"
                    });
                }

                var response = new RolResponseDto
                {
                    RolId = rol.RolId,
                    Nombre = rol.Nombre,
                    Descripcion = rol.Descripcion,
                    Estado = rol.Estado,
                    PermisosIds = rol.RolPermisos?
                                    .Select(rp => rp.PermisoId)
                                    .Distinct()
                                    .ToList() ?? new List<int>(),
                    Permisos = rol.RolPermisos?
                                    .Where(rp => rp.Permiso != null)
                                    .Select(rp => rp.Permiso!.Nombre)
                                    .OrderBy(n => n)
                                    .ToList() ?? new List<string>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar rol: {ex.Message}"
                });
            }
        }

        // ============================================================
        // REGISTRAR NUEVO ROL (acepta asignación de permisos)
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<RolResponseDto>> RegistrarRol([FromBody] CrearRolDto dto)
        {
            try
            {
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del rol es obligatorio"
                    });
                }

                // Verificar que el nombre del rol no exista
                var rolExiste = await _context.Rols
                    .AnyAsync(r => r.Nombre.ToLower() == dto.Nombre.ToLower());

                if (rolExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Ya existe un rol con ese nombre"
                    });
                }

                // Crear nuevo rol
                var nuevoRol = new Rol
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Estado = true
                };

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Rols.Add(nuevoRol);
                await _context.SaveChangesAsync();

                // Asignar permisos válidos (si los hay)
                if (dto.PermisosIds != null && dto.PermisosIds.Count > 0)
                {
                    var permisosValidos = await _context.Permisos
                        .Where(p => dto.PermisosIds.Contains(p.PermisoId))
                        .Select(p => p.PermisoId)
                        .ToListAsync();

                    foreach (var permisoId in permisosValidos.Distinct())
                    {
                        _context.RolPermisos.Add(new RolPermiso
                        {
                            RolId = nuevoRol.RolId,
                            PermisoId = permisoId
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Obtener permisos asignados para la respuesta
                var asignados = await _context.RolPermisos
                    .Where(rp => rp.RolId == nuevoRol.RolId)
                    .Include(rp => rp.Permiso)
                    .ToListAsync();

                var response = new RolResponseDto
                {
                    RolId = nuevoRol.RolId,
                    Nombre = nuevoRol.Nombre,
                    Descripcion = nuevoRol.Descripcion,
                    Estado = nuevoRol.Estado,
                    PermisosIds = asignados.Select(a => a.PermisoId).Distinct().ToList(),
                    Permisos = asignados.Where(a => a.Permiso != null).Select(a => a.Permiso!.Nombre).OrderBy(n => n).ToList()
                };

                return CreatedAtAction(nameof(BuscarRol), new { id = nuevoRol.RolId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al registrar rol: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR ROL (sincroniza permisos)
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult<RolResponseDto>> ActualizarRol(int id, [FromBody] ActualizarRolDto dto)
        {
            try
            {
                var rol = await _context.Rols
                    .Include(r => r.RolPermisos!)
                        .ThenInclude(rp => rp.Permiso)
                    .FirstOrDefaultAsync(r => r.RolId == id);

                if (rol == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rol no encontrado"
                    });
                }

                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del rol es obligatorio"
                    });
                }

                // Verificar que el nuevo nombre no exista (si cambió)
                if (rol.Nombre.ToLower() != dto.Nombre.ToLower())
                {
                    var nombreExiste = await _context.Rols
                        .AnyAsync(r => r.Nombre.ToLower() == dto.Nombre.ToLower() && r.RolId != id);

                    if (nombreExiste)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Ya existe un rol con ese nombre"
                        });
                    }
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                // Actualizar datos
                rol.Nombre = dto.Nombre;
                rol.Descripcion = dto.Descripcion;
                rol.Estado = dto.Estado;

                await _context.SaveChangesAsync();

                // Sincronizar permisos
                var existentes = await _context.RolPermisos
                    .Where(rp => rp.RolId == id)
                    .ToListAsync();

                var existentesIds = existentes.Select(e => e.PermisoId).ToHashSet();

                var nuevosIds = dto.PermisosIds != null
                    ? dto.PermisosIds.Distinct().ToList()
                    : new List<int>();

                // Permisos a eliminar
                var aEliminar = existentes.Where(e => !nuevosIds.Contains(e.PermisoId)).ToList();
                if (aEliminar.Any())
                {
                    _context.RolPermisos.RemoveRange(aEliminar);
                }

                // Permisos a añadir (solo si existen en tabla Permisos)
                var aAñadirIds = nuevosIds.Where(pid => !existentesIds.Contains(pid)).ToList();
                if (aAñadirIds.Any())
                {
                    var permisosValidos = await _context.Permisos
                        .Where(p => aAñadirIds.Contains(p.PermisoId))
                        .Select(p => p.PermisoId)
                        .ToListAsync();

                    foreach (var permisoId in permisosValidos.Distinct())
                    {
                        _context.RolPermisos.Add(new RolPermiso
                        {
                            RolId = id,
                            PermisoId = permisoId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Recuperar permisos actuales para la respuesta
                var actuales = await _context.RolPermisos
                    .Where(rp => rp.RolId == id)
                    .Include(rp => rp.Permiso)
                    .ToListAsync();

                var response = new RolResponseDto
                {
                    RolId = rol.RolId,
                    Nombre = rol.Nombre,
                    Descripcion = rol.Descripcion,
                    Estado = rol.Estado,
                    PermisosIds = actuales.Select(a => a.PermisoId).Distinct().ToList(),
                    Permisos = actuales.Where(a => a.Permiso != null).Select(a => a.Permiso!.Nombre).OrderBy(n => n).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar rol: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ELIMINAR ROL
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarRol(int id)
        {
            try
            {
                var rol = await _context.Rols.FindAsync(id);

                if (rol == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Rol no encontrado"
                    });
                }

                // Proteger rol 'Super Admin' de borrado
                if (!string.IsNullOrWhiteSpace(rol.Nombre) && rol.Nombre.Equals("Super Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar el rol 'Super Admin'"
                    });
                }

                // Verificar si el rol tiene usuarios asociados
                var tieneUsuarios = await _context.Usuarios.AnyAsync(u => u.RolId == id);

                if (tieneUsuarios)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar el rol porque tiene usuarios asociados"
                    });
                }

                // Verificar si el rol tiene permisos asociados
                var tienePermisos = await _context.RolPermisos.AnyAsync(rp => rp.RolId == id);

                if (tienePermisos)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar el rol porque tiene permisos asociados"
                    });
                }

                _context.Rols.Remove(rol);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Rol eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar rol: {ex.Message}"
                });
            }
        }
    }
}
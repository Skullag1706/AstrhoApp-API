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
    public class ProveedoresController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public ProveedoresController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Proveedores
        [HttpGet]
        [Authorize(Policy = "perm:Proveedores")]
        public async Task<IActionResult> ObtenerProveedores([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Proveedors.AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(p => 
                        p.Nombre.ToLower().Contains(busqueda) || 
                        p.Documento.ToLower().Contains(busqueda) ||
                        p.PersonaContacto.ToLower().Contains(busqueda) ||
                        p.Correo.ToLower().Contains(busqueda)
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var proveedores = await query
                    .OrderByDescending(p => p.ProveedorId)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();

                var resultados = proveedores.Select(p => new ProveedorResponseDto
                {
                    ProveedorId = p.ProveedorId,
                    TipoProveedor = p.TipoProveedor,
                    Nombre = p.Nombre,
                    TipoDocumento = p.TipoDocumento,
                    Documento = p.Documento,
                    PersonaContacto = p.PersonaContacto,
                    Correo = p.Correo,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion,
                    Departamento = p.Departamento,
                    Ciudad = p.Ciudad,
                    Estado = p.Estado ?? false
                }).ToList();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = resultados 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener proveedores: {ex.Message}" });
            }
        }

        // GET: api/Proveedores/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "perm:Proveedores")]
        public async Task<IActionResult> ObtenerProveedor(int id)
        {
            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor == null)
                return NotFound(new { success = false, message = "Proveedor no encontrado" });

            var dto = new ProveedorResponseDto
            {
                ProveedorId = proveedor.ProveedorId,
                TipoProveedor = proveedor.TipoProveedor,
                Nombre = proveedor.Nombre,
                TipoDocumento = proveedor.TipoDocumento,
                Documento = proveedor.Documento,
                PersonaContacto = proveedor.PersonaContacto,
                Correo = proveedor.Correo,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,
                Departamento = proveedor.Departamento,
                Ciudad = proveedor.Ciudad,
                Estado = proveedor.Estado ?? false
            };

            return Ok(new { success = true, data = dto });
        }

        // POST: api/Proveedores
        [HttpPost]
        [Authorize(Policy = "perm:Proveedores")]
        public async Task<IActionResult> CrearProveedor([FromBody] CrearProveedorDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Datos inv�lidos" });

            // Validaciones m�nimas
            if (string.IsNullOrWhiteSpace(dto.TipoProveedor) || string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { success = false, message = "TipoProveedor y Nombre son requeridos" });

            // Verificar documento �nico si se proporciona
            if (!string.IsNullOrWhiteSpace(dto.Documento))
            {
                var existente = await _context.Proveedors.AnyAsync(p => p.Documento == dto.Documento);
                if (existente)
                    return BadRequest(new { success = false, message = "Documento ya registrado" });
            }

            var proveedor = new Proveedor
            {
                TipoProveedor = dto.TipoProveedor,
                Nombre = dto.Nombre,
                TipoDocumento = dto.TipoDocumento,
                Documento = dto.Documento,
                PersonaContacto = dto.Persona_Contacto,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Departamento = dto.Departamento,
                Ciudad = dto.Ciudad,
                Estado = dto.Estado ?? true
            };

            try
            {
                await _context.Proveedors.AddAsync(proveedor);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Proveedor creado correctamente", proveedorId = proveedor.ProveedorId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al crear proveedor: {ex.Message}" });
            }
        }

        // PUT: api/Proveedores/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Proveedores")]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] ActualizarProveedorDto dto)
        {
            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor == null)
                return NotFound(new { success = false, message = "Proveedor no encontrado" });

            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Documento) && dto.Documento != proveedor.Documento)
                {
                    var existeDoc = await _context.Proveedors.AnyAsync(p => p.Documento == dto.Documento && p.ProveedorId != id);
                    if (existeDoc)
                        return BadRequest(new { success = false, message = "Documento ya registrado por otro proveedor" });

                    proveedor.Documento = dto.Documento;
                }

                if (!string.IsNullOrWhiteSpace(dto.TipoProveedor))
                    proveedor.TipoProveedor = dto.TipoProveedor;

                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                    proveedor.Nombre = dto.Nombre;

                if (dto.TipoDocumento != null)
                    proveedor.TipoDocumento = dto.TipoDocumento;

                if (dto.Persona_Contacto != null)
                    proveedor.PersonaContacto = dto.Persona_Contacto;

                if (dto.Correo != null)
                    proveedor.Correo = dto.Correo;

                if (dto.Telefono != null)
                    proveedor.Telefono = dto.Telefono;

                if (dto.Direccion != null)
                    proveedor.Direccion = dto.Direccion;

                if (dto.Departamento != null)
                    proveedor.Departamento = dto.Departamento;

                if (dto.Ciudad != null)
                    proveedor.Ciudad = dto.Ciudad;

                if (dto.Estado.HasValue)
                    proveedor.Estado = dto.Estado.Value;

                _context.Proveedors.Update(proveedor);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Proveedor actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al actualizar proveedor: {ex.Message}" });
            }
        }

        // DELETE: api/Proveedores/{id}
        // Realiza eliminaci�n f�sica del registro
        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Proveedores")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor == null)
                return NotFound(new { success = false, message = "Proveedor no encontrado" });

            try
            {
                _context.Proveedors.Remove(proveedor);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Proveedor eliminado correctamente" });
            }
            catch (DbUpdateException dbEx)
            {
                // Probablemente restricci�n de FK: informar al cliente para que tome otra acci�n
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede eliminar el proveedor porque existen registros relacionados. Considere desactivarlo (soft-delete) o eliminar/actualizar las dependencias primero.",
                    detail = dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al eliminar proveedor: {ex.Message}" });
            }
        }
    }
}
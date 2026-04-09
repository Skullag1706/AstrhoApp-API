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
    public class ClientesController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public ClientesController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTAR TODOS LOS CLIENTES
        // ============================================================
        [HttpGet]
        [Authorize(Policy = "perm:Clientes")]
        public async Task<ActionResult> ListarClientes([FromQuery] int pagina = 1, [FromQuery] string? buscar = null)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                int registrosPorPagina = 5;

                var query = _context.Clientes.AsQueryable();

                // Buscador dinámico
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    string busqueda = buscar.Trim().ToLower();
                    query = query.Where(c => 
                        c.Nombre.ToLower().Contains(busqueda) || 
                        c.DocumentoCliente.ToLower().Contains(busqueda) ||
                        (c.Telefono != null && c.Telefono.ToLower().Contains(busqueda))
                    );
                }

                var totalRegistros = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                var clientes = await query
                    .OrderBy(c => c.Nombre)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .Select(c => new ClienteResponseDto
                    {
                        DocumentoCliente = c.DocumentoCliente,
                        TipoDocumento = c.TipoDocumento,
                        UsuarioId = c.UsuarioId,
                        Nombre = c.Nombre,
                        Telefono = c.Telefono,
                        Dirección = c.Dirección,
                        Estado = c.Estado
                    })
                    .ToListAsync();

                return Ok(new 
                { 
                    success = true, 
                    totalRegistros, 
                    totalPaginas, 
                    paginaActual = pagina,
                    registrosPorPagina,
                    data = clientes 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al listar clientes: {ex.Message}"
                });
            }
        }

        // ============================================================
        // BUSCAR CLIENTE POR DOCUMENTO
        // ============================================================
        [HttpGet("{documentoCliente}")]
        [Authorize(Policy = "perm:Clientes")]
        public async Task<ActionResult<ClienteResponseDto>> BuscarCliente(string documentoCliente)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.DocumentoCliente == documentoCliente);

                if (cliente == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cliente no encontrado"
                    });
                }

                var response = new ClienteResponseDto
                {
                    DocumentoCliente = cliente.DocumentoCliente,
                    UsuarioId = cliente.UsuarioId,
                    TipoDocumento = cliente.TipoDocumento,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Dirección = cliente.Dirección,
                    Estado = cliente.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar cliente: {ex.Message}"
                });
            }
        }

        // ============================================================
        // REGISTRAR NUEVO CLIENTE
        // ============================================================
        [HttpPost]
        [Authorize(Policy = "perm:Clientes")]
        public async Task<ActionResult<ClienteResponseDto>> RegistrarCliente([FromBody] CrearClienteDto dto)
        {
            try
            {
                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del cliente es obligatorio"
                    });
                }

                // Validar que el documento no esté vacío
                if (string.IsNullOrWhiteSpace(dto.DocumentoCliente))
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
                var clienteExiste = await _context.Clientes
                    .AnyAsync(c => c.DocumentoCliente == dto.DocumentoCliente);

                if (clienteExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Ya existe un cliente con ese documento"
                    });
                }

                // Crear nuevo cliente
                var nuevoCliente = new Cliente
                {
                    DocumentoCliente = dto.DocumentoCliente,
                    UsuarioId = dto.UsuarioId,
                    TipoDocumento = dto.TipoDocumento,
                    Nombre = dto.Nombre,
                    Telefono = dto.Telefono,
                    Dirección = dto.Dirección,
                    Estado = true
                };

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();

                // Cargar el usuario para la respuesta
                await _context.Entry(nuevoCliente).Reference(c => c.Usuario).LoadAsync();

                var response = new ClienteResponseDto
                {
                    DocumentoCliente = nuevoCliente.DocumentoCliente,
                    UsuarioId = nuevoCliente.UsuarioId,
                    TipoDocumento = nuevoCliente.TipoDocumento,
                    Nombre = nuevoCliente.Nombre,
                    Telefono = nuevoCliente.Telefono,
                    Dirección = nuevoCliente.Dirección,
                    Estado = nuevoCliente.Estado
                };

                return CreatedAtAction(nameof(BuscarCliente), new { documentoCliente = nuevoCliente.DocumentoCliente }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al registrar cliente: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ACTUALIZAR CLIENTE
        // ============================================================
        [HttpPut("{documentoCliente}")]
        [Authorize(Policy = "perm:Clientes")]
        public async Task<ActionResult<ClienteResponseDto>> ActualizarCliente(string documentoCliente, [FromBody] ActualizarClienteDto dto)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.DocumentoCliente == documentoCliente);

                if (cliente == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cliente no encontrado"
                    });
                }

                // Validar que el nombre no esté vacío
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El nombre del cliente es obligatorio"
                    });
                }

                // Actualizar datos
                cliente.TipoDocumento = dto.TipoDocumento;
                cliente.Nombre = dto.Nombre;
                cliente.Telefono = dto.Telefono;
                cliente.Dirección = dto.Dirección;
                cliente.Estado = dto.Estado;

                await _context.SaveChangesAsync();

                var response = new ClienteResponseDto
                {
                    DocumentoCliente = cliente.DocumentoCliente,
                    UsuarioId = cliente.UsuarioId,
                    TipoDocumento = cliente.TipoDocumento,
                    Nombre = cliente.Nombre,
                    Telefono = cliente.Telefono,
                    Dirección = cliente.Dirección,
                    Estado = cliente.Estado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar cliente: {ex.Message}"
                });
            }
        }

        // ============================================================
        // ELIMINAR CLIENTE
        // ============================================================
        [HttpDelete("{documentoCliente}")]
        [Authorize(Policy = "perm:Clientes")]
        public async Task<ActionResult> EliminarCliente(string documentoCliente)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(documentoCliente);

                if (cliente == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Cliente no encontrado"
                    });
                }

                // Verificar si el cliente tiene citas
                var tieneCitas = await _context.Agenda.AnyAsync(a => a.DocumentoCliente == documentoCliente);
                // Verificar si el cliente tiene ventas
                var tieneVentas = await _context.Venta.AnyAsync(v => v.DocumentoCliente == documentoCliente);

                if (tieneCitas || tieneVentas)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se puede eliminar el cliente porque tiene registros asociados (citas o ventas)"
                    });
                }

                var usuarioId = cliente.UsuarioId;
                var usuario = await _context.Usuarios.FindAsync(usuarioId);

                _context.Clientes.Remove(cliente);
                
                if (usuario != null)
                {
                    _context.Usuarios.Remove(usuario);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cliente eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar cliente: {ex.Message}"
                });
            }
        }
    }
}
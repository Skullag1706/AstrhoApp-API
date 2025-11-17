using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using AstrhoApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public PedidosController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        [HttpGet("productos")]
        public async Task<ActionResult> ObtenerProductos()
        {
            var productos = await _context.Productos
                .Where(p => p.Estado == true && p.Stock > 0)
                .Select(p => new
                {
                    p.ProductoId,
                    p.Nombre,
                    p.Descripcion,
                    p.PrecioVenta,
                    p.Stock,
                    Categoria = p.Categoria.Nombre,
                    Marca = p.Marca.Nombre
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpPost]
        public async Task<ActionResult<PedidoResponseDto>> CrearPedido([FromBody] CrearPedidoDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validar cliente
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.DocumentoCliente == dto.DocumentoCliente);
                if (!clienteExiste)
                    return BadRequest("Cliente no encontrado");

                decimal subtotal = 0;
                var detallesVenta = new List<DetalleVentum>();

                // Validar productos y calcular subtotal
                foreach (var detalle in dto.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto == null)
                        return BadRequest($"Producto {detalle.ProductoId} no encontrado");

                    if (producto.Stock < detalle.Cantidad)
                        return BadRequest($"Stock insuficiente para {producto.Nombre}");

                    decimal subtotalDetalle = producto.PrecioVenta * detalle.Cantidad;
                    subtotal += subtotalDetalle;

                    detallesVenta.Add(new DetalleVentum
                    {
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = producto.PrecioVenta,
                        Subtotal = subtotalDetalle
                    });

                    // Actualizar stock
                    producto.Stock -= detalle.Cantidad;
                }

                // Calcular total con descuento
                decimal descuento = subtotal * (dto.PorcentajeDescuento / 100);
                decimal total = subtotal - descuento;

                // Obtener estado "Pendiente"
                var estadoPendiente = await _context.Estados.FirstOrDefaultAsync(e => e.Nombre == "Pendiente");
                if (estadoPendiente == null)
                    return BadRequest("Estado 'Pendiente' no configurado");

                // Generar ID de venta
                string ventaId = $"V-{DateTime.Now:yyyyMMddHHmmss}";

                // Crear venta
                var venta = new Ventum
                {
                    VentaId = ventaId,
                    DocumentoCliente = dto.DocumentoCliente,
                    EstadoId = estadoPendiente.EstadoId,
                    FechaRegistro = DateTime.Now,
                    MetodoPago = dto.MetodoPago,
                    Subtotal = subtotal,
                    PorcentajeDescuento = dto.PorcentajeDescuento,
                    Total = total
                };

                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();

                // Agregar detalles
                foreach (var detalle in detallesVenta)
                {
                    detalle.VentaId = ventaId;
                    _context.DetalleVenta.Add(detalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new PedidoResponseDto
                {
                    VentaId = ventaId,
                    FechaRegistro = venta.FechaRegistro ?? DateTime.Now,
                    Subtotal = subtotal,
                    Total = total,
                    Estado = "Pendiente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("mis-pedidos/{documentoCliente}")]
        public async Task<ActionResult> ObtenerMisPedidos(int documentoCliente)
        {
            var pedidos = await _context.Venta
                .Include(v => v.Estado)
                .Where(v => v.DocumentoCliente == documentoCliente)
                .Select(v => new
                {
                    v.VentaId,
                    v.FechaRegistro,
                    v.Subtotal,
                    v.Total,
                    Estado = v.Estado.Nombre,
                    v.MetodoPago
                })
                .OrderByDescending(v => v.FechaRegistro)
                .ToListAsync();

            return Ok(pedidos);
        }
    }
}
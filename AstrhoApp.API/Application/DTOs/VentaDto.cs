using System;
using System.Collections.Generic;

namespace AstrhoApp.API.DTOs
{
    public class VentaDto
    {
        public string VentaId { get; set; } = string.Empty;
        public string DocumentoCliente { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;

        // Informaci�n del empleado asociada a la venta (si aplica)
        public string EmpleadoDocumento { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;

        public int MetodopagoId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public bool Estado { get; set; }
        public string Observacion { get; set; } = string.Empty;

        // Nueva lista para servicios asociados a la cita de esta venta
        public List<VentaServicioDto> Servicios { get; set; } = new List<VentaServicioDto>();
    }

    public class VentaServicioDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }

    public class ActualizarVentaDto
    {
        // S�lo se permite editar descuento y estado seg�n lo solicitado
        public bool? Estado { get; set; }
        public string? Observacion { get; set; }
    }

    public class CrearVentaDto
    {
        public string DocumentoCliente { get; set; } = string.Empty;
        public string DocumentoEmpleado { get; set; } = string.Empty;
        public int MetodopagoId { get; set; }
        public string? Observacion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        // Se puede incluir una lista de AgendasIds si se desea asociar la venta a una o varias citas existentes
        public List<int>? AgendasIds { get; set; }
        public List<CrearDetalleVentaDto> Detalles { get; set; } = new List<CrearDetalleVentaDto>();
    }

    public class CrearDetalleVentaDto
    {
        public int ServicioId { get; set; }
        public decimal Precio { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace AstrhoApp.API.DTOs
{
    public class DetalleCompraItemDto
    {
        public int InsumoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CrearCompraDto
    {
        public int ProveedorId { get; set; }
        public decimal? Iva { get; set; } // porcentaje (ej. 19 => 19%)
        public string? Observacion { get; set; }
        public List<DetalleCompraItemDto> Items { get; set; } = new();
    }

    public class ActualizarCompraDto
    {
        // Actualizaci�n limitada: proveedor, iva y estado.
        public int? ProveedorId { get; set; }
        public decimal? Iva { get; set; }
        public string? Observacion { get; set; }
        public bool? Estado { get; set; }
    }

    public class DetalleCompraResponseDto
    {
        public int DetalleCompraId { get; set; }
        public int InsumoId { get; set; }
        public string InsumoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CompraResponseDto
    {
        public int CompraId { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public decimal Iva { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string? Observacion { get; set; }
        public bool Estado { get; set; }
        public List<DetalleCompraResponseDto> Detalles { get; set; } = new();
    }
}
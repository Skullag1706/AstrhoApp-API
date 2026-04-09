using System;
using System.Collections.Generic;

namespace AstrhoApp.API.DTOs
{
    public class DetalleEntregaItemDto
    {
        public int InsumoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearEntregaDto
    {
        public int UsuarioId { get; set; }
        public string DocumentoEmpleado { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; }
        public List<DetalleEntregaItemDto> Detalles { get; set; } = new();
    }

    public class ActualizarEntregaDto
    {
        public int? UsuarioId { get; set; }
        public string? DocumentoEmpleado { get; set; }
        public DateTime? FechaEntrega { get; set; }
        // Se maneja por ID conectado a la tabla de estados
        public int? EstadoId { get; set; }
        public List<DetalleEntregaItemDto>? Detalles { get; set; }
    }

    public class EntregaListDto
    {
        public int EntregainsumoId { get; set; }
        public int UsuarioId { get; set; }
        public string DocumentoEmpleado { get; set; } = string.Empty;
        public DateTime FechaCreado { get; set; }
        public DateTime FechaEntrega { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public int? EstadoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CantidadItems { get; set; }

    }

    public class EntregaResponseDto
    {
        public int EntregainsumoId { get; set; }
        public int UsuarioId { get; set; }
        public string DocumentoEmpleado { get; set; } = string.Empty;
        public DateTime FechaCreado { get; set; }
        public DateTime FechaEntrega { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public int? EstadoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<DetalleEntregaItemDto> Detalles { get; set; } = new();
    }
}
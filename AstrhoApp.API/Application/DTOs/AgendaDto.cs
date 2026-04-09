using AstrhoApp.API.DTOs;

namespace AstrhoApp.API.DTOs
{

    // ===== CREAR CITA CON SERVICIOS (WEB) =====
    public class CrearCitaDto
    {
        public string DocumentoCliente { get; set; } = null!;
        public string DocumentoEmpleado { get; set; } = null!;
        public DateOnly FechaCita { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public int MetodoPagoId { get; set; }
        public string? Observaciones { get; set; }
        public List<int> ServiciosIds { get; set; } = new();
    }

    public class ActualizarCitaDto
    {
        public string DocumentoCliente { get; set; } = string.Empty;
        public string DocumentoEmpleado { get; set; } = string.Empty;
        public DateOnly FechaCita { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public int? MetodoPagoId { get; set; }
        public string? Observaciones { get; set; }
        public List<int>? ServiciosIds { get; set; }
        public int? EstadoId { get; set; }
    }

    public class CitaClienteDto
    {
        public int AgendaId { get; set; }
        public DateOnly FechaCita { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public string DocumentoEmpleado { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        // Ahora contiene los nombres de los servicios tal como en ObtenerTodasLasCitas
        public List<string> Servicios { get; set; } = new();

        // Nuevo: método de pago (nombre)
        public string MetodoPago { get; set; } = string.Empty;
    }

}
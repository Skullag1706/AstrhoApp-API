namespace AstrhoApp.API.DTOs
{
    // ===== CREAR EMPLEADO =====
    public class CrearEmpleadoDto
    {
        public string DocumentoEmpleado { get; set; } = null!; // VARCHAR(20) - PK
        public int UsuarioId { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
    }

    // ===== ACTUALIZAR EMPLEADO =====
    public class ActualizarEmpleadoDto
    {
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
        public bool Estado { get; set; }
    }

    // ===== RESPUESTA DE EMPLEADO =====
    public class EmpleadoResponseDto
    {
        public string DocumentoEmpleado { get; set; } = null!;
        public int UsuarioId { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
        public bool Estado { get; set; }
    }

    // ===== LISTA DE EMPLEADOS =====
    public class EmpleadoListDto
    {
        public string DocumentoEmpleado { get; set; } = null!;
        public string TipoDocumento { get; set; } = null!;
        public int? UsuarioId { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Dirección { get; set; }
        public bool Estado { get; set; }
    }

    // ===== HISTORIAL DEL EMPLEADO =====
    public class EmpleadoHistorialDto
    {
        public EmpleadoResponseDto Empleado { get; set; } = null!;
        public List<CitaEmpleadoDto> Citas { get; set; } = new();
        public List<EntregaInsumoDto> EntregasInsumos { get; set; } = new();
        public ResumenEmpleadoDto Resumen { get; set; } = null!;
    }

    public class CitaEmpleadoDto
    {
        public int AgendaId { get; set; }
        public DateOnly FechaCita { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public string Estado { get; set; } = null!;
        public string ClienteNombre { get; set; } = null!;
        public List<string> Servicios { get; set; } = new();
    }

    public class EntregaInsumoDto
    {
        public int EntregaInsumoId { get; set; }
        public DateTime FechaCreado { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public int? EstadoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CantidadInsumos { get; set; }
    }

    public class ResumenEmpleadoDto
    {
        public int TotalCitas { get; set; }
        public int CitasCompletadas { get; set; }
        public int CitasPendientes { get; set; }
        public int TotalEntregas { get; set; }
        public DateOnly? UltimaCita { get; set; }
        public DateOnly? ProximaCita { get; set; }
    }
}
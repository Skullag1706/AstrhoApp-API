namespace AstrhoApp.API.DTOs
{
    public class CrearAgendaDto
    {
        public int DocumentoCliente { get; set; }
        public int DocumentoEmpleado { get; set; }
        public DateOnly FechaCita { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public List<int> ServiciosIds { get; set; } = new();
    }

    public class AgendaResponseDto
    {
        public int AgendaId { get; set; }
        public DateOnly FechaCita { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string Estado { get; set; } = null!;
        public string? ClienteNombre { get; set; }
        public string? EmpleadoNombre { get; set; }
        public List<string> Servicios { get; set; } = new();
    }
}

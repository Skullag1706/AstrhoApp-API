using System.Collections.Generic;
using System.Text.Json;

namespace AstrhoApp.API.DTOs
{
    public class HorarioEmpleadoResponseDto
    {
        public int HorarioEmpleadoId { get; set; }
        public int HorarioDiaId { get; set; }
        public int HorarioId { get; set; }
        public string HorarioNombre { get; set; } = null!;
        public string DocumentoEmpleado { get; set; } = null!;
        public string? EmpleadoNombre { get; set; }
        public string? DiaSemana { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFin { get; set; }
    }

    public class CrearHorarioEmpleadoDto
    {
        public int HorarioDiaId { get; set; }
        public string DocumentoEmpleado { get; set; } = null!;
    }

    public class ModificarHorarioEmpleadoDto
    {
        public int? HorarioDiaId { get; set; }
        public string? DocumentoEmpleado { get; set; }
    }

    public class AsignacionMasivaDto
    {
        public List<AsignacionDiaDto> Dias { get; set; } = new();
    }

    public class AsignacionDiaDto
    {
        public int HorarioDiaId { get; set; }
        public List<JsonElement> Empleados { get; set; } = new();
    }
}

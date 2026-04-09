using System;
using System.Collections.Generic;

namespace AstrhoApp.API.DTOs
{
    public class HorarioResponseDto
    {
        public int HorarioId { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Estado { get; set; }
        public List<HorarioDiaDto> Dias { get; set; } = new();
    }

    public class HorarioDiaDto
    {
        public int HorarioDiaId { get; set; }
        public string DiaSemana { get; set; } = null!;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
    }

    public class CrearHorarioDto
    {
        public string Nombre { get; set; } = null!;
        public bool Estado { get; set; } = true;
        public List<CrearHorarioDiaDto> Dias { get; set; } = new();
    }

    public class CrearHorarioDiaDto
    {
        public string DiaSemana { get; set; } = null!;
        public string HoraInicio { get; set; } = null!; // "HH:mm"
        public string HoraFin { get; set; } = null!;    // "HH:mm"
    }

    public class ModificarHorarioDiaDto
    {
        public int? HorarioDiaId { get; set; } // Opcional: si viene se actualiza, si no se crea
        public string DiaSemana { get; set; } = null!;
        public string HoraInicio { get; set; } = null!; // "HH:mm"
        public string HoraFin { get; set; } = null!;    // "HH:mm"
    }

    public class ModificarHorarioDto
    {
        public string? Nombre { get; set; }
        public bool? Estado { get; set; }
        public List<ModificarHorarioDiaDto>? Dias { get; set; } // Merge inteligente de días
    }
}

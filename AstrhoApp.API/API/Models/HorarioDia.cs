using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class HorarioDia
{
    public int HorarioDiaId { get; set; }

    public int HorarioId { get; set; }

    public string DiaSemana { get; set; } = null!;

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public virtual Horario Horario { get; set; } = null!;

    public virtual ICollection<HorarioEmpleado> HorarioEmpleados { get; set; } = new List<HorarioEmpleado>();
}

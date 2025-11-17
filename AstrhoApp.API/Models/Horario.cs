using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Horario
{
    public int HorarioId { get; set; }

    public string DiaSemana { get; set; } = null!;

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<HorarioEmpleado> HorarioEmpleados { get; set; } = new List<HorarioEmpleado>();
}

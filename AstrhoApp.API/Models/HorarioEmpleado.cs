using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class HorarioEmpleado
{
    public int HorarioEmpleadoId { get; set; }

    public int HorarioId { get; set; }

    public int EmpleadoId { get; set; }

    public virtual Empleado Empleado { get; set; } = null!;

    public virtual Horario Horario { get; set; } = null!;
}

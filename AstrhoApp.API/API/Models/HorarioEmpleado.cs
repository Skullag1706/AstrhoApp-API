using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class HorarioEmpleado
{
    public int HorarioEmpleadoId { get; set; }

    public int HorarioDiaId { get; set; }

    public string DocumentoEmpleado { get; set; } = null!;

    public virtual Empleado DocumentoEmpleadoNavigation { get; set; } = null!;

    public virtual HorarioDia HorarioDia { get; set; } = null!;
}

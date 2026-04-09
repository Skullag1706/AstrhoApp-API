using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Horario
{
    public int HorarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<HorarioDia> HorarioDias { get; set; } = new List<HorarioDia>();
}

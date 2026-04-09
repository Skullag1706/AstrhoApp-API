using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Estado
{
    public int EstadoId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Agendum> Agenda { get; set; } = new List<Agendum>();

    public virtual ICollection<Entregainsumo> Entregainsumos { get; set; } = new List<Entregainsumo>();
}

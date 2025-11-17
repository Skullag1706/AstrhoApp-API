using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class ServicioAgendum
{
    public int ServicioAgendaId { get; set; }

    public int ServicioId { get; set; }

    public int AgendaId { get; set; }

    public virtual Agendum Agenda { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;
}

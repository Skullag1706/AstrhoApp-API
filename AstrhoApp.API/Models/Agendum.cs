using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Agendum
{
    public int AgendaId { get; set; }

    public int DocumentoCliente { get; set; }

    public int DocumentoEmpleado { get; set; }

    public DateOnly FechaCita { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public string? Estado { get; set; }

    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public virtual Cliente DocumentoClienteNavigation { get; set; } = null!;

    public virtual Empleado DocumentoEmpleadoNavigation { get; set; } = null!;

    public virtual ICollection<ServicioAgendum> ServicioAgenda { get; set; } = new List<ServicioAgendum>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}

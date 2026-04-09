using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Agendum
{
    public int AgendaId { get; set; }

    public string DocumentoCliente { get; set; } = null!;

    public string DocumentoEmpleado { get; set; } = null!;

    public DateOnly FechaCita { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public int MetodopagoId { get; set; }

    public string? Observaciones { get; set; }

    public int? EstadoId { get; set; }

    public string? VentaId { get; set; }

    public virtual Cliente DocumentoClienteNavigation { get; set; } = null!;

    public virtual Empleado DocumentoEmpleadoNavigation { get; set; } = null!;

    public virtual Estado? Estado { get; set; }

    public virtual MetodoPago Metodopago { get; set; } = null!;

    public virtual ICollection<ServicioAgendum> ServicioAgenda { get; set; } = new List<ServicioAgendum>();

    public virtual Ventum? Venta { get; set; }
}

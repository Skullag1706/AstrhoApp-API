using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Entregainsumo
{
    public int EntregainsumoId { get; set; }

    public int UsuarioId { get; set; }

    public string DocumentoEmpleado { get; set; } = null!;

    public DateTime? FechaCreado { get; set; }

    public DateTime FechaEntrega { get; set; }

    public DateTime? FechaCompletado { get; set; }

    public int? EstadoId { get; set; }

    public virtual ICollection<DetalleEntrega> DetalleEntregas { get; set; } = new List<DetalleEntrega>();

    public virtual Empleado DocumentoEmpleadoNavigation { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual Estado? Estado { get; set; }
}

using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Entregainsumo
{
    public int EntregainsumoId { get; set; }

    public DateTime? FechaCreado { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public DateTime? FechaCompletado { get; set; }

    public int EmpleadoId { get; set; }

    public int UsuarioId { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<DetalleProducto> DetalleProductos { get; set; } = new List<DetalleProducto>();

    public virtual Empleado Empleado { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}

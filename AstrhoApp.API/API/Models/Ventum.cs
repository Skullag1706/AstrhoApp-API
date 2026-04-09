using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Ventum
{
    public string VentaId { get; set; } = null!;

    public string DocumentoCliente { get; set; } = null!;

    public string DocumentoEmpleado { get; set; } = null!;

    public bool Estado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public int MetodopagoId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public string? observacion { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();

    public virtual ICollection<Agendum> Agenda { get; set; } = new List<Agendum>();

    public virtual Cliente DocumentoClienteNavigation { get; set; } = null!;

    public virtual MetodoPago Metodopago { get; set; } = null!;
}

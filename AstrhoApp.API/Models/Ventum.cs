using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Ventum
{
    public string VentaId { get; set; } = null!;

    public int DocumentoCliente { get; set; }

    public int EstadoId { get; set; }

    public int? AgendaId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? MetodoPago { get; set; }

    public decimal Subtotal { get; set; }

    public decimal? PorcentajeDescuento { get; set; }

    public decimal Total { get; set; }

    public virtual Agendum? Agenda { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Cliente DocumentoClienteNavigation { get; set; } = null!;

    public virtual Estado Estado { get; set; } = null!;
}

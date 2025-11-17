using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Compra
{
    public int CompraId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public int ProveedorId { get; set; }

    public decimal? Iva { get; set; }

    public decimal? Descuento { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual Proveedor Proveedor { get; set; } = null!;
}

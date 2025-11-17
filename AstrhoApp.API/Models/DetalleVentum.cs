using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class DetalleVentum
{
    public int DetalleVentaId { get; set; }

    public string VentaId { get; set; } = null!;

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Ventum Venta { get; set; } = null!;
}

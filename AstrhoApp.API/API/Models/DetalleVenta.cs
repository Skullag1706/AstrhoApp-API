using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class DetalleVenta
{
    public int DetalleVentaId { get; set; }

    public string VentaId { get; set; } = null!;

    public int ServicioId { get; set; }

    public decimal Precio { get; set; }

    public virtual Ventum Venta { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;
}

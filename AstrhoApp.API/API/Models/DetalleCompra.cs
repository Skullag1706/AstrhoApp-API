using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class DetalleCompra
{
    public int DetalleCompraId { get; set; }

    public int CompraId { get; set; }

    public int InsumoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Compra Compra { get; set; } = null!;

    public virtual Insumo Insumo { get; set; } = null!;
}

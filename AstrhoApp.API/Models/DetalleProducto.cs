using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class DetalleProducto
{
    public int DetalleProductoId { get; set; }

    public int EntregainsumoId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public virtual Entregainsumo Entregainsumo { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}

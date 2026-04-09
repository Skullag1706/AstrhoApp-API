using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class DetalleEntrega
{
    public int DetalleEntregaId { get; set; }

    public int EntregainsumoId { get; set; }

    public int InsumoId { get; set; }

    public int Cantidad { get; set; }

    public virtual Entregainsumo Entregainsumo { get; set; } = null!;

    public virtual Insumo Insumo { get; set; } = null!;
}

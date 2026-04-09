using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Insumo
{
    public int InsumoId { get; set; }

    public string Sku { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int CategoriaId { get; set; }

    public bool? Estado { get; set; }

    public int Stock { get; set; }

    public virtual Categorium Categoria { get; set; } = null!;

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual ICollection<DetalleEntrega> DetalleEntregas { get; set; } = new List<DetalleEntrega>();
}

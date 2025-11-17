using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public string Sku { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int CategoriaId { get; set; }

    public int MarcaId { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal PrecioVenta { get; set; }

    public int? Stock { get; set; }

    public bool? Estado { get; set; }

    public virtual Categorium Categoria { get; set; } = null!;

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual ICollection<DetalleProducto> DetalleProductos { get; set; } = new List<DetalleProducto>();

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual ICollection<Imagen> Imagens { get; set; } = new List<Imagen>();

    public virtual Marca Marca { get; set; } = null!;
}

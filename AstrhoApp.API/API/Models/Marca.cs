using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Marca
{
    public int MarcaId { get; set; }

    public string Nombre { get; set; } = null!;
}

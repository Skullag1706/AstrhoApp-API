using System;
using System.Collections.Generic;

namespace AstrhoApp.API.Models;

public partial class Acceso
{
    public int AccesoId { get; set; }

    public int? UsuarioId { get; set; }

    public virtual Usuario? Usuario { get; set; }
}

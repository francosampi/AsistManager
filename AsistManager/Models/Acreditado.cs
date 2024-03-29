﻿using System;
using System.Collections.Generic;

namespace AsistManager.Models;

public partial class Acreditado
{
    public int Id { get; set; }

    public int? IdEvento { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Dni { get; set; }

    public string? Cuit { get; set; }

    public bool Habilitado { get; set; }

    public string? Celular { get; set; }

    public string? Grupo { get; set; }

    public bool Alta { get; set; }

    public virtual ICollection<Egreso> Egresos { get; set; } = new List<Egreso>();

    public virtual Evento? IdEventoNavigation { get; set; }

    public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}

﻿using System;
using System.Collections.Generic;

namespace AsistManager.Models;

public partial class Ingreso
{
    public int Id { get; set; }

    public int IdAcreditado { get; set; }

    public DateTime FechaOperacion { get; set; }

    public virtual ICollection<Egreso> Egresos { get; set; } = new List<Egreso>();

    public virtual Acreditado IdAcreditadoNavigation { get; set; } = null!;
}

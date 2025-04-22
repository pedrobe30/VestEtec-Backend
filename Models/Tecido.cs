using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("tecido")]
public partial class Tecido
{
    [Key]
    [Column("ID_tecido", TypeName = "int(11)")]
    public int IdTecido { get; set; }

    [Column("tipo")]
    [StringLength(100)]
    public string? Tipo { get; set; }

    [InverseProperty("IdTecidoNavigation")]
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("modelos")]
public partial class Modelo
{
    [Key]
    [Column("ID_modelo", TypeName = "int(11)")]
    public int IdModelo { get; set; }

    [Column("modelo")]
    [StringLength(100)]
    public string? Modelo1 { get; set; }

    [InverseProperty("IdModeloNavigation")]
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}

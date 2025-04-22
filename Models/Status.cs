using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("status")]
public partial class Status
{
    [Key]
    [Column("ID_status", TypeName = "int(11)")]
    public int IdStatus { get; set; }

    [Column("descricao")]
    [StringLength(15)]
    public string? Descricao { get; set; }

    [InverseProperty("IdStatusNavigation")]
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}

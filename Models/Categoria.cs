using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("categorias")]
public partial class Categoria
{
    [Key]
    [Column("ID_categoria", TypeName = "int(11)")]
    public int IdCategoria { get; set; }

    [Column("categoria")]
    [StringLength(100)]
    public string? Categoria1 { get; set; }

    [InverseProperty("IdCategoriaNavigation")]
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}

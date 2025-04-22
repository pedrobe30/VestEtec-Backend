using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("itensencomendados")]
public partial class Itensencomendado
{
    [Key]
    [Column("id_item", TypeName = "int(11)")]
    public int IdItem { get; set; }

    [Column("Id_encomenda", TypeName = "int(11)")]
    public int IdEncomenda { get; set; }

    [Column("id_produto", TypeName = "int(11)")]
    public int IdProduto { get; set; }

    [Column("quantidade", TypeName = "int(11)")]
    public int Quantidade { get; set; }

    [Column("preco_uni")]
    [Precision(10, 2)]
    public decimal PrecoUni { get; set; }
}

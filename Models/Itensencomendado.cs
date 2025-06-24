// Models/Itensencomendado.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("itensencomendados")]
[Index("IdEncomenda", Name = "fk_encomenda_item")]
[Index("IdProduto", Name = "fk_produto_item")]
public partial class Itensencomendado
{
    [Key]
    [Column("id_item", TypeName = "int(11)")]
    public int IdItem { get; set; }

    [Column("Id_encomenda", TypeName = "int(11)")]
    public int IdEncomenda { get; set; }

    [Column("id_produto", TypeName = "int(11)")]
    public int IdProduto { get; set; }

    [Column("quantidade_encomendado", TypeName = "int(11)")]
    public int Quantidade { get; set; }

    [ForeignKey("IdEncomenda")]
    [InverseProperty("Itensencomendados")]
    public virtual Encomenda IdEncomendaNavigation { get; set; } = null!;

    [ForeignKey("IdProduto")]
    [InverseProperty("Itensencomendados")]
    public virtual Produto IdProdutoNavigation { get; set; } = null!;

    // CORREÇÃO: Propriedade Tamanho adicionada para armazenar o tamanho do item encomendado.
    [Required]
    [Column("tamanho")]
    [StringLength(10)]
    public string Tamanho { get; set; }
}
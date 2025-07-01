// Models/Produto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("produto")]
[Index("IdCategoria", Name = "fk_categoria")]
[Index("IdModelo", Name = "fk_modelo")]
[Index("IdStatus", Name = "fk_status")]
[Index("IdTecido", Name = "fk_tecido")]
public partial class Produto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID_prod", TypeName = "int(11)")]
    public int IdProd { get; set; }

    [Column("preco")]
    [Precision(10, 2)]
    public decimal Preco { get; set; }

    [Column("ID_categoria", TypeName = "int(11)")]
    public int IdCategoria { get; set; }

    [Column("ID_modelo", TypeName = "int(11)")]
    public int IdModelo { get; set; }

    [Column("ID_tecido", TypeName = "int(11)")]
    public int? IdTecido { get; set; }

    [Column("ID_status", TypeName = "int(11)")]
    public int IdStatus { get; set; }

    [Column("img_url")]
    [StringLength(255)]
    public string? ImgUrl { get; set; }

    [Column("descricao")]
    [StringLength(255)]
    public string? descricao { get; set; }

    // Navegação para outras entidades
    [ForeignKey("IdCategoria")]
    [InverseProperty("Produtos")]
    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    [ForeignKey("IdModelo")]
    [InverseProperty("Produtos")]
    public virtual Modelo IdModeloNavigation { get; set; } = null!;

    [ForeignKey("IdStatus")]
    [InverseProperty("Produtos")]
    public virtual Status IdStatusNavigation { get; set; } = null!;

    [ForeignKey("IdTecido")]
    [InverseProperty("Produtos")]
    public virtual Tecido? IdTecidoNavigation { get; set; }

    // Coleções de entidades relacionadas
    [InverseProperty("IdProdutoNavigation")]
    public virtual ICollection<Estoque> Estoque { get; set; } = new List<Estoque>();

    [InverseProperty("IdProdutoNavigation")]
    public virtual ICollection<Itensencomendado> Itensencomendados { get; set; } = new List<Itensencomendado>();

    // NOVA NAVEGAÇÃO PARA MÚLTIPLAS IMAGENS
    [InverseProperty("IdProdutoNavigation")]
    public virtual ICollection<ProdutoImagem> ProdutoImagens { get; set; } = new List<ProdutoImagem>();
}
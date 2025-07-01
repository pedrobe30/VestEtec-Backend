// Models/ProdutoImagem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Vestetec_App.Models;

[Table("produto_imagem")]
public partial class ProdutoImagem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID_produto_imagem", TypeName = "int(11)")]
    public int IdProdutoImagem { get; set; }

    [Column("ID_produto", TypeName = "int(11)")]
    public int IdProduto { get; set; }

    [Column("img_url")]
    [StringLength(255)]
    [Required]
    public string ImgUrl { get; set; } = string.Empty;

    [Column("ordem_exibicao", TypeName = "tinyint(4)")]
    public byte OrdemExibicao { get; set; } = 1;

    [Column("is_principal", TypeName = "tinyint(1)")]
    public bool IsPrincipal { get; set; } = false;

    [Column("data_criacao", TypeName = "timestamp")]
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    [ForeignKey("IdProduto")]
    [InverseProperty("ProdutoImagens")]
    public virtual Produto IdProdutoNavigation { get; set; } = null!;
}
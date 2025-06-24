// Models/Estoque.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Vestetec_App.Models;

[Table("estoque")]
public partial class Estoque
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_estoque", TypeName = "int(11)")]
    public int IdEstoque { get; set; }

    [Column("id_produto", TypeName = "int(11)")]
    public int IdProduto { get; set; }

    [Required]
    [Column("tamanho")]
    [StringLength(10)]
    public string Tamanho { get; set; } = null!;

    [Column("quantidade", TypeName = "int(11)")]
    public int Quantidade { get; set; }

    [ForeignKey("IdProduto")]
    [InverseProperty("Estoque")]
    public virtual Produto IdProdutoNavigation { get; set; } = null!;
}

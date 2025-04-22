using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("historico_estoque")]
[Index("IdAdm", Name = "fk_adm")]
public partial class HistoricoEstoque
{
    [Key]
    [Column("ID", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("alteracao")]
    [StringLength(255)]
    public string Alteracao { get; set; } = null!;

    [Column("data_alteracao")]
    public DateOnly DataAlteracao { get; set; }

    [Column("ID_adm", TypeName = "int(11)")]
    public int IdAdm { get; set; }

    [ForeignKey("IdAdm")]
    [InverseProperty("HistoricoEstoques")]
    public virtual Admin IdAdmNavigation { get; set; } = null!;
}

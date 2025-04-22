using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("admins")]
[Index("IdEmpresa", Name = "id_Empresa")]
public partial class Admin
{
    [Key]
    [Column("id_adm", TypeName = "int(11)")]
    public int IdAdm { get; set; }

    [Column("senha")]
    [StringLength(255)]
    public string Senha { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("nome")]
    [StringLength(200)]
    public string Nome { get; set; } = null!;

    [Column("id_Empresa", TypeName = "int(11)")]
    public int IdEmpresa { get; set; }

    [InverseProperty("IdAdmNavigation")]
    public virtual ICollection<HistoricoEstoque> HistoricoEstoques { get; set; } = new List<HistoricoEstoque>();

    [ForeignKey("IdEmpresa")]
    [InverseProperty("Admins")]
    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;
}

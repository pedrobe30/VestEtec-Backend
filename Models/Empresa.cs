using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("empresas")]
public partial class Empresa
{
    [Key]
    [Column("id_empresa", TypeName = "int(11)")]
    public int IdEmpresa { get; set; }

    [Column("nome")]
    [StringLength(200)]
    public string Nome { get; set; } = null!;

    [InverseProperty("IdEmpresaNavigation")]
    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();
}

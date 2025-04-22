using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("aluno")]
[Index("EmailAlu", Name = "email_alu", IsUnique = true)]
public partial class Aluno
{
    [Key]
    [Column("id_aluno", TypeName = "int(11)")]
    public int IdAluno { get; set; }

    [Column("rm", TypeName = "int(5)")]
    public int Rm { get; set; }

    [Column("nome_alu")]
    [StringLength(200)]
    public string NomeAlu { get; set; } = null!;

    [Column("email_alu")]
    [StringLength(200)]
    public string EmailAlu { get; set; } = null!;

    [Column("senha_alu")]
    [StringLength(255)]
    public string SenhaAlu { get; set; } = null!;

    [Column("Id_esc", TypeName = "int(11)")]
    public int? IdEsc { get; set; }
    
    [Column("email_verificado", TypeName = "tinyint(1)")]
    public bool EmailVerificado {get; set;} = false;

    

    [InverseProperty("IdAlunoNavigation")]
    public virtual ICollection<Encomenda> Encomenda { get; set; } = new List<Encomenda>();
}

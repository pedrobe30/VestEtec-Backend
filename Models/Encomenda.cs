using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("encomendas")]
[Index("IdAluno", Name = "fk_alunos_enco")]
[Index("IdEscola", Name = "fk_escola")]
public partial class Encomenda
{
    [Key]
    [Column("id_encomenda", TypeName = "int(11)")]
    public int IdEncomenda { get; set; }

    [Column("Id_aluno", TypeName = "int(11)")]
    public int IdAluno { get; set; }

    [Column("data_encomenda", TypeName = "datetime")]
    public DateTime DataEncomenda { get; set; }

    [Column("preco_encomenda")]
    [Precision(10, 2)]
    public decimal PrecoEncomenda { get; set; }

    [Column("situacao")]
    [StringLength(100)]
    public string? Situacao { get; set; }

    [Column("Id_escola", TypeName = "int(11)")]
    public int IdEscola { get; set; }

    [ForeignKey("IdAluno")]
    [InverseProperty("Encomenda")]
    public virtual Aluno IdAlunoNavigation { get; set; } = null!;

    [ForeignKey("IdEscola")]
    [InverseProperty("Encomenda")]
    public virtual Escola IdEscolaNavigation { get; set; } = null!;
}

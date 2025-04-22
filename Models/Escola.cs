using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Table("escolas")]
public partial class Escola
{
    [Key]
    [Column("id_escola", TypeName = "int(11)")]
    public int IdEscola { get; set; }

    [Column("nome_esc")]
    [StringLength(200)]
    public string NomeEsc { get; set; } = null!;

    [InverseProperty("IdEscolaNavigation")]
    public virtual ICollection<Encomenda> Encomenda { get; set; } = new List<Encomenda>();
}

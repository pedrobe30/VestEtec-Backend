using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Models;

[Keyless]
[Table("teste")]
public partial class Teste
{
    [Column("nome")]
    [StringLength(100)]
    public string Nome { get; set; } = null!;
}

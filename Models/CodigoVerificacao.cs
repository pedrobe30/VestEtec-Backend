using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Vestetec_App.Models
{
    [Table("codigo_verificacao")]
    public class CodigoVerificacao
    {
        [Key]
        [Column("id", TypeName = "int(11)")]
        public int Id { get; set; }
        
        [Column("email")]
        [StringLength(200)]
        public string Email { get; set; } = null!;
        
        [Column("codigo")]
        [StringLength(6)]
        public string Codigo { get; set; } = null!;
        
        [Column("data_expiracao")]
        public DateTime DataExpiracao { get; set; }
        
        [Column("ativo")]
        public bool Ativo { get; set; } = true;
    }
}
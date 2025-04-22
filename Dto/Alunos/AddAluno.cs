using Backend_Vestetec_App.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dto.Aluno
{

    public class CriacaoAluno
    {
        [Column("rm", TypeName = "int(5)")]
        [Required(ErrorMessage = "Informe seu RM")]
        public int Rm { get; set; }

        [Column("nome_alu")]
        [StringLength(200)]
        [Required(ErrorMessage = "Seu nome")]
        public string NomeAlu { get; set; } = null!;

        [Column("email_alu")]
        [StringLength(200)]
        [Required(ErrorMessage = "Informe seu Email")]
        [EmailAddress(ErrorMessage = "Email Invalido")]
        public string EmailAlu { get; set; } = null!;

        [Column("senha_alu")]
        [StringLength(255)]
        [MinLength(6, ErrorMessage = "A senha deve ter no minimo 6 caracteres")]
        public string SenhaAlu { get; set; } = null!;

        [Column("id_esc")]
        [Required]
        public int IdEsc { get; set; }

    
    }
}
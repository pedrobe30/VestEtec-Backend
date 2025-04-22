using Backend_Vestetec_App.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Dto.Aluno
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Prencha seu Email")]
        [EmailAddress(ErrorMessage = "Email Invalido")]
        public string email {get; set;} = null!;


        [Required(ErrorMessage = "Senha é obrigatória")]
        public string senha {get; set;} = null!;
    }

    public class AlteracaoSenhaDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string email { get; set; } = null!;

        [Required(ErrorMessage = "Senha atual é obrigatória")]
        public string senhaAtual { get; set; } = null!;

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres")]
        public string novaSenha { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;

namespace Backend_Vestetec_App.DTOs
{
    public class LoginAdmDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string Senha { get; set; } = null!;
    }

    public class LoginAdmResponseDto
    {
        public int IdAdm { get; set; }
        public string Email { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public int IdEmpresa { get; set; }
        public string? NomeEmpresa { get; set; }
        public DateTime DataLogin { get; set; }
        public string Token { get; set; } = null!;
        public string Message { get; set; } = "Login realizado com sucesso";
    }

    public class AlterarSenhaDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha atual é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha atual deve ter entre 6 e 255 caracteres")]
        public string SenhaAtual { get; set; } = null!;

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Nova senha deve ter entre 6 e 255 caracteres")]
        public string NovaSenha { get; set; } = null!;

        [Required(ErrorMessage = "Confirmação da nova senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "A confirmação da senha não confere com a nova senha")]
        public string ConfirmarNovaSenha { get; set; } = null!;
    }

    public class EsqueciSenhaDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Nova senha deve ter entre 6 e 255 caracteres")]
        public string NovaSenha { get; set; } = null!;

        [Required(ErrorMessage = "Confirmação da nova senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "A confirmação da senha não confere com a nova senha")]
        public string ConfirmarNovaSenha { get; set; } = null!;

        [Required(ErrorMessage = "Código preciso é obrigatório")]
        [StringLength(10, ErrorMessage = "Código preciso deve ter no máximo 10 caracteres")]
        public string CodigoPreciso { get; set; } = null!;
    }

     public class ValidateAdminTokenDto
    {
        public int IdAdm { get; set; }
        public string Email { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public int IdEmpresa { get; set; }
        public string? NomeEmpresa { get; set; }
        public bool IsValid { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
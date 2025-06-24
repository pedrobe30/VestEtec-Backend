using System.ComponentModel.DataAnnotations;

namespace Backend_Vestetec_App.DTOs
{
    public class AdminCreateDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string Senha { get; set; } = null!;

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "ID da empresa é obrigatório")]
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "Código preciso é obrigatório")]
        [StringLength(10, ErrorMessage = "Código preciso deve ter no máximo 10 caracteres")]
        public string CodigoPreciso { get; set; } = null!;
    }

    public class AdminUpdateDto
    {
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string? Email { get; set; }

        [StringLength(255, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 255 caracteres")]
        public string? Senha { get; set; }

        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string? Nome { get; set; }

        public int? IdEmpresa { get; set; }
    }

    public class AdminResponseDto
    {
        public int IdAdm { get; set; }
        public string Email { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public int IdEmpresa { get; set; }
        public string? NomeEmpresa { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
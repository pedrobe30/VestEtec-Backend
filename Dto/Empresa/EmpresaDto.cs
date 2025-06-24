using System.ComponentModel.DataAnnotations;

namespace Backend_Vestetec_App.DTOs;

public class EmpresaDto
{
    public int IdEmpresa { get; set; }

    [Required(ErrorMessage = "O nome da empresa é obrigatório")]
    [StringLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = null!;
}

public class EmpresaCreateDto
{
    [Required(ErrorMessage = "O nome da empresa é obrigatório")]
    [StringLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = null!;
}

public class EmpresaUpdateDto
{
    [Required(ErrorMessage = "O nome da empresa é obrigatório")]
    [StringLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; } = null!;
}

// DTO específico para dropdown (apenas dados essenciais)
public class EmpresaDropdownDto
{
    public int IdEmpresa { get; set; }
    public string Nome { get; set; } = null!;
}
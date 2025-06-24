using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Backend_Vestetec_App.DTOs
{
    /// <summary>
    /// DTO para representar tamanho e quantidade
    /// </summary>
    public class TamanhoQuantidadeDto
    {
        [Required(ErrorMessage = "O tamanho é obrigatório")]
        [StringLength(10, ErrorMessage = "O tamanho deve ter no máximo 10 caracteres")]
        public string Tamanho { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int Quantidade { get; set; } = 0;
    }

    /// <summary>
    /// DTO simplificado para criação de produtos
    /// </summary>
    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O tecido é obrigatório")]
        public int IdTecido { get; set; }

        [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A imagem é obrigatória")]
        public IFormFile Imagem { get; set; } = null!;

        [Required(ErrorMessage = "Os tamanhos e quantidades são obrigatórios")]
        public string TamanhosQuantidadesJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualização de produtos
    /// </summary>
    public class ProdutoUpdateDto
    {
        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O tecido é obrigatório")]
        public int IdTecido { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [Range(1, 2, ErrorMessage = "Status deve ser 1 (Disponível) ou 2 (Indisponível)")]
        public int IdStatus { get; set; }

        [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres")]
        public string? Descricao { get; set; }

        public IFormFile? Imagem { get; set; }

        [Required(ErrorMessage = "Os tamanhos e quantidades são obrigatórios")]
        public string TamanhosQuantidadesJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta - contém todos os dados do produto
    /// </summary>
    public class ProdutoResponseDto
    {
        public int IdProd { get; set; }
        public decimal Preco { get; set; }
        public int IdCategoria { get; set; }
        public int IdModelo { get; set; }
        public int IdTecido { get; set; }
        public int IdStatus { get; set; }
        public string? Descricao { get; set; }
        public string? ImgUrl { get; set; }

        // Nomes para exibição
        public string? CategoriaNome { get; set; }
        public string? ModeloNome { get; set; }
        public string? TecidoNome { get; set; }
        public string? StatusNome { get; set; }

        // Lista de tamanhos e quantidades para exibição
        public List<TamanhoQuantidadeDto> TamanhosQuantidades { get; set; } = new List<TamanhoQuantidadeDto>();
    }

    /// <summary>
    /// DTO completo para comunicação controller/service (criação)
    /// </summary>
    public class ProdutoCompletoDto
    {
        public decimal Preco { get; set; }
        public int IdCategoria { get; set; }
        public int IdModelo { get; set; }
        public int IdTecido { get; set; }
        public string? Descricao { get; set; }
        public IFormFile Imagem { get; set; } = null!;
        public List<TamanhoQuantidadeDto> TamanhosQuantidades { get; set; } = new();
    }

    /// <summary>
    /// DTO completo para comunicação controller/service (atualização)
    /// </summary>
    public class ProdutoUpdateCompletoDto
    {
        public decimal Preco { get; set; }
        public int IdCategoria { get; set; }
        public int IdModelo { get; set; }
        public int IdTecido { get; set; }
        public int IdStatus { get; set; }
        public string? Descricao { get; set; }
        public IFormFile? Imagem { get; set; }
        public List<TamanhoQuantidadeDto> TamanhosQuantidades { get; set; } = new();
    }

    /// <summary>
    /// DTO principal - mantido para compatibilidade
    /// </summary>
    public class ProdutoDto
    {
        public int IdProd { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O tecido é obrigatório")]
        public int IdTecido { get; set; }

        public int IdStatus { get; set; }
        public string? Descricao { get; set; }
        public string? ImgUrl { get; set; }

        // Campos auxiliares
        public string? CategoriaNome { get; set; }
        public string? ModeloNome { get; set; }
        public string? TecidoNome { get; set; }
        public string? StatusNome { get; set; }

        public List<TamanhoQuantidadeDto> TamanhosQuantidades { get; set; } = new List<TamanhoQuantidadeDto>();

        public Dictionary<string, int> EstoquePorTamanho
        {
            get => TamanhosQuantidades?.ToDictionary(t => t.Tamanho, t => t.Quantidade) ?? new Dictionary<string, int>();
            set
            {
                if (value != null)
                {
                    TamanhosQuantidades = value.Select(kv => new TamanhoQuantidadeDto
                    {
                        Tamanho = kv.Key,
                        Quantidade = kv.Value
                    }).ToList();
                }
            }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IFormFile? Imagem { get; set; }
    }
}
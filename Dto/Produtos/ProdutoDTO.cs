using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Backend_Vestetec_App.DTOs
{
    /// <summary>
    /// DTO para representar uma imagem do produto
    /// </summary>
    public class ProdutoImagemDto
    {
        public int? IdProdutoImagem { get; set; }
        public string ImgUrl { get; set; } = string.Empty;
        public byte OrdemExibicao { get; set; } = 1;
        public bool IsPrincipal { get; set; } = false;
    }

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
    /// DTO para criação de produtos com múltiplas imagens
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

        [Required(ErrorMessage = "Pelo menos uma imagem é obrigatória")]
        [MaxLength(4, ErrorMessage = "Máximo de 4 imagens permitidas")]
        public List<IFormFile> Imagens { get; set; } = new List<IFormFile>();

        [Required(ErrorMessage = "Os tamanhos e quantidades são obrigatórios")]
        public string TamanhosQuantidadesJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualização de produtos com múltiplas imagens
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

        // Para novas imagens a serem adicionadas
        [MaxLength(4, ErrorMessage = "Máximo de 4 imagens permitidas")]
        public List<IFormFile>? NovasImagens { get; set; }

        // Para controlar quais imagens manter/remover (IDs das imagens existentes)
        public List<int>? ImagensParaManter { get; set; }

        [Required(ErrorMessage = "Os tamanhos e quantidades são obrigatórios")]
        public string TamanhosQuantidadesJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta - contém todos os dados do produto com múltiplas imagens
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
        
        // MANTIDO PARA COMPATIBILIDADE - IMAGEM PRINCIPAL
        public string? ImgUrl { get; set; }

        // NOVA PROPRIEDADE - LISTA DE TODAS AS IMAGENS
        public List<ProdutoImagemDto> Imagens { get; set; } = new List<ProdutoImagemDto>();

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
        public List<IFormFile> Imagens { get; set; } = new List<IFormFile>();
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
        public List<IFormFile>? NovasImagens { get; set; }
        public List<int>? ImagensParaManter { get; set; }
        public List<TamanhoQuantidadeDto> TamanhosQuantidades { get; set; } = new();
    }

    // MANTIDOS PARA COMPATIBILIDADE
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
        public List<ProdutoImagemDto> Imagens { get; set; } = new List<ProdutoImagemDto>();

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
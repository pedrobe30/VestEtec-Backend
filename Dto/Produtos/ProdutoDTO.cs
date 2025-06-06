using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Backend_Vestetec_App.DTOs
{
    /// <summary>
    /// DTO simplificado para cria��o e atualiza��o de produtos
    /// Remove campos que s�o gerados automaticamente pelo sistema
    /// </summary>
    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "O pre�o � obrigat�rio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O pre�o deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque � obrigat�ria")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantEstoque { get; set; }

        [Required(ErrorMessage = "A categoria � obrigat�ria")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo � obrigat�rio")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O Tecido � obrigat�rio")]
        public int? IdTecido { get; set; }

        // Campo de imagem para upload - o �nico campo de arquivo necess�rio
        [Required(ErrorMessage = "A imagem � obrigat�ria")]
        public IFormFile Imagem { get; set; }
    }

    /// <summary>
    /// DTO para atualiza��o de produtos - permite alterar o status
    /// </summary>
    public class ProdutoUpdateDto
    {
        [Required(ErrorMessage = "O pre�o � obrigat�rio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O pre�o deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque � obrigat�ria")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantEstoque { get; set; }

        [Required(ErrorMessage = "A categoria � obrigat�ria")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo � obrigat�rio")]
        public int IdModelo { get; set; }

        public int? IdTecido { get; set; }

        [Required(ErrorMessage = "O status � obrigat�rio")]
        [Range(1, 2, ErrorMessage = "Status deve ser 1 (Dispon�vel) ou 2 (Indispon�vel)")]
        public int IdStatus { get; set; }

        // Imagem � opcional na atualiza��o - se n�o enviar, mant�m a atual
        public IFormFile? Imagem { get; set; }
    }

    /// <summary>
    /// DTO para resposta - cont�m todos os dados do produto, incluindo nomes das refer�ncias
    /// Este DTO � usado quando retornamos dados para o cliente
    /// </summary>
    public class ProdutoResponseDto
    {
        public int IdProd { get; set; }
        public decimal Preco { get; set; }
        public int QuantEstoque { get; set; }
        public int IdCategoria { get; set; }
        public int IdModelo { get; set; }
        public int? IdTecido { get; set; }
        public int IdStatus { get; set; }

        // URL da imagem - gerada automaticamente pelo sistema
        public string? ImgUrl { get; set; }

        // Nomes para exibi��o - facilita o trabalho do frontend
        public string? CategoriaNome { get; set; }
        public string? ModeloNome { get; set; }
        public string? TecidoNome { get; set; }
        public string? StatusNome { get; set; }
    }

    /// <summary>
    /// DTO principal - usado para entrada e sa�da de dados
    /// </summary>
    public class ProdutoDto
    {
        // Campo auto-increment - n�o aparece no formul�rio de cria��o
        public int IdProd { get; set; }

        [Required(ErrorMessage = "O pre�o � obrigat�rio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O pre�o deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque � obrigat�ria")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantEstoque { get; set; }

        [Required(ErrorMessage = "A categoria � obrigat�ria")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo � obrigat�rio")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O Tecido � obrigat�rio")]
        public int? IdTecido { get; set; }

        public int IdStatus { get; set; }

        // ImgUrl � gerada automaticamente pelo service
        public string? ImgUrl { get; set; }

        // Campos auxiliares - preenchidos automaticamente pelo service baseado nos IDs
        public string? CategoriaNome { get; set; }
        public string? ModeloNome { get; set; }
        public string? TecidoNome { get; set; }
        public string? StatusNome { get; set; }

        // Campo de imagem para upload - usado apenas na entrada (POST/PUT)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IFormFile? Imagem { get; set; }
    }
}
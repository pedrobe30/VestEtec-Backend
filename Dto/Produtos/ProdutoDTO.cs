using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Backend_Vestetec_App.DTOs
{
    /// <summary>
    /// DTO simplificado para criação e atualização de produtos
    /// Remove campos que são gerados automaticamente pelo sistema
    /// </summary>
    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantEstoque { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O Tecido é obrigatório")]
        public int? IdTecido { get; set; }

        // Campo de imagem para upload - o único campo de arquivo necessário
        [Required(ErrorMessage = "A imagem é obrigatória")]
        public IFormFile Imagem { get; set; }
    }

    /// <summary>
    /// DTO para atualização de produtos - permite alterar o status
    /// </summary>
    public class ProdutoUpdateDto
    {
        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantEstoque { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        public int IdModelo { get; set; }

        public int? IdTecido { get; set; }

        [Required(ErrorMessage = "O status é obrigatório")]
        [Range(1, 2, ErrorMessage = "Status deve ser 1 (Disponível) ou 2 (Indisponível)")]
        public int IdStatus { get; set; }

        // Imagem é opcional na atualização - se não enviar, mantém a atual
        public IFormFile? Imagem { get; set; }
    }

    /// <summary>
    /// DTO para resposta - contém todos os dados do produto, incluindo nomes das referências
    /// Este DTO é usado quando retornamos dados para o cliente
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

        // Nomes para exibição - facilita o trabalho do frontend
        public string? CategoriaNome { get; set; }
        public string? ModeloNome { get; set; }
        public string? TecidoNome { get; set; }
        public string? StatusNome { get; set; }
    }

    /// <summary>
    /// DTO principal - usado para entrada e saída de dados
    /// </summary>
    public class ProdutoDto
    {
        // Campo auto-increment - não aparece no formulário de criação
        public int IdProd { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A quantidade em estoque é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantEstoque { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "O modelo é obrigatório")]
        public int IdModelo { get; set; }

        [Required(ErrorMessage = "O Tecido é obrigatório")]
        public int? IdTecido { get; set; }

        public int IdStatus { get; set; }

        // ImgUrl é gerada automaticamente pelo service
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
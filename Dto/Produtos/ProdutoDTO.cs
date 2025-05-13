// DTOs/ProdutoDto.cs
namespace Backend_Vestetec_App.DTOs
{
    public class ProdutoDto
    {
        public int IdProd { get; set; }
        public decimal Preco { get; set; }
        public int QuantEstoque { get; set; }
        public int IdCategoria { get; set; }
        public int IdModelo { get; set; }
        public int? IdTecido { get; set; }
        public int IdStatus { get; set; }
        public string? ImgUrl { get; set; }
        public string? Descricao { get; set; }
        
   
        public string? CategoriaNome { get; set; }
        public string? ModeloNome { get; set; }
        public string? TecidoNome { get; set; }
        public string? StatusNome { get; set; }
    }


    public class ProdutoCompletoDto
    {
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public int QuantEstoque { get; set; }
        public int IdCategoria { get; set; }
        public int IdModelo { get; set; }
        public int? IdTecido { get; set; }
        public string? ImgUrl { get; set; }
       
    }
}
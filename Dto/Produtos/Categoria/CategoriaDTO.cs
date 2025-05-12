using System.ComponentModel.DataAnnotations;

namespace Backend_Vestetec_App.DTOs
{
    public class createCategoriaDTO
    {
        [Required(ErrorMessage = "O nome da categoria ém obrigatório")]
        [StringLength(100, ErrorMessage = "No maximo 100 caracteres")]
        public string Categoria {get; set;}
    }

    public class UpdateCategoriaDTO
    {
        [Required(ErrorMessage = "O nome da categoria ém obrigatório")]
        [StringLength(100, ErrorMessage = "No maximo 100 caracteres")]
        public string Categoria {get; set;}
    }

    public class CategoriaResponseDTO
    {
        public int IdCategoria {get; set;}
        public string Categoria {get; set;}
    }
}
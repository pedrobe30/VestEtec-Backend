using System.ComponentModel.DataAnnotations;

 namespace Backend_Vestetec_App.DTOs
 {
    public class AddModelos
    {

    
    [Required(ErrorMessage = "Por favor é obrigatorio o nome do modelo")]
    [StringLength(100, ErrorMessage = "No maximo 100 caracteres")]
    public string Modelo {get; set;}

    }

    public class ModeloUpdate
    {
        [Required(ErrorMessage = "Obrigatorío")]
        [StringLength(100, ErrorMessage = "100 caracteres no maximo")]
        public string Modelo {get; set;}

    }

    public class ModeloResponseDTO
    {
        public int IdModelo {get; set;}
        public string Modelo {get; set;}
        
    }
 }
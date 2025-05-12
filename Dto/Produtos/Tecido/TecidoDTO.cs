using System.ComponentModel.DataAnnotations;

 namespace Backend_Vestetec_App.DTOs
 {
    public class AddTecido
    {

    
    [Required(ErrorMessage = "Por favor é obrigatorio o nome do modelo")]
    [StringLength(100, ErrorMessage = "No maximo 100 caracteres")]
    public string tipoTecido {get; set;}

    }

    public class TecidoUpdate
    {
        [Required(ErrorMessage = "Obrigatorío")]
        [StringLength(100, ErrorMessage = "100 caracteres no maximo")]
        public string tipoTecido {get; set;}

    }

    public class TecidoResponseDTO
    {
        public int IdTecido {get; set;}
        public string tipoTecido {get; set;}
        
    }
 }
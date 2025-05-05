using Backend_Vestetec_App.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dto.Escola 
{
    public class EscolaDto
    {
        [Column("id_escola")]
        public int IdEsc {get; set;}

        [Column("nome_esc")]
        public string nome_esc {get; set;} = string.Empty;
    }
}
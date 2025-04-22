using Backend_Vestetec_App.Models;

namespace Dto.Aluno
{
    public class verificacaoEmailDto
    {
        public string Email {get; set;} = null!;
        public string Codigo {get; set;} = null!;
    }

    // public class CriacaoAlunoVerificado : CriacaoAluno
    // {
    //     public string CodigoVerificacao {get; set;} = null!;
    // }
}
 using Backend_Vestetec_App.Models;
 using Dto.Aluno;
 
 namespace Alunos.Services
 {
    public interface IAlunoInterface
    {
        Task<ResponseModel<Aluno>> AdicionarAluno(CriacaoAluno criacaoAluno);
        // Task<ResponseModel<bool>> VerificarEmail(string email);
        Task<ResponseModel<Aluno>> AtualizarSenha(int id, string novaSenha);

        
    }


}
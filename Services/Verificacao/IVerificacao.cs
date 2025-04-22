using Backend_Vestetec_App.Models;
using Dto.Aluno;

namespace Backend_Vestetec_App.Services
{
    public interface IVerificacaoService
    {
        // Gerar e salvar código de verificação para um email
        Task<string> GerarCodigoVerificacao(string email);
        
        // Verificar se o código corresponde ao email
        Task<ResponseModel<bool>> VerificarCodigo(verificacaoEmailDto verificacao);
        
        // Obter o status de verificação de um email
        Task<bool> EmailEstaVerificado(string email);
        
        // Marcar um email como verificado após confirmação bem-sucedida
        Task<ResponseModel<bool>> MarcarEmailComoVerificado(string email);
    }
}
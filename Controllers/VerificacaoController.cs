using Microsoft.AspNetCore.Mvc;
using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Services;
using Dto.Aluno;
using Mailjet.Client.TransactionalEmails;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificacaoController : ControllerBase
    {
        private readonly IVerificacaoService _verificacaoService;
        private readonly IEnviarEmail _emailService;

        public VerificacaoController (IVerificacaoService verificacaoService, IEnviarEmail emailService)
        {
            _verificacaoService = verificacaoService;
            _emailService = emailService;
        }

        [HttpPost("enviar-codigo")]
        public async Task<ActionResult<ResponseModel<bool>>> EnviarCodigoVerificacao([FromBody] string email)
        {
            var resposta = new ResponseModel<bool>();
            try
            {
                await _verificacaoService.GerarCodigoVerificacao(email);
                resposta.Dados = true;
                resposta.Mensagem = "Código de verificação enviado com sucesso!";
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro ao enviar código de verificação: {ex.Message}";
                return BadRequest(resposta);
            }
        }

        [HttpPost("verificar-email")]
        public async Task<ActionResult<ResponseModel<bool>>> VerificarEmail(verificacaoEmailDto verificacao)
        {
            var resultado = await _verificacaoService.VerificarCodigo(verificacao);
            
            if (!resultado.status)
                return BadRequest(resultado);
            
            return Ok(resultado);
        }

        [HttpGet("status/{email}")]
        public async Task<ActionResult<ResponseModel<bool>>> VerificarStatusEmail(string email)
        {
            var resposta = new ResponseModel<bool>();
            try
            {
                resposta.Dados = await _verificacaoService.EmailEstaVerificado(email);
                resposta.Mensagem = resposta.Dados 
                    ? "Email verificado." 
                    : "Email não verificado.";
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro ao verificar status do email: {ex.Message}";
                return BadRequest(resposta);
            }
        }

        [HttpPost("reenviar-codigo")]
        public async Task<ActionResult<ResponseModel<bool>>> ReenviarCodigoVerificacao([FromBody] string email)
        {
            var resposta = new ResponseModel<bool>();
            try
            {
                // Verificar se o email já está verificado
                if (await _verificacaoService.EmailEstaVerificado(email))
                {
                    resposta.status = false;
                    resposta.Mensagem = "Este email já está verificado.";
                    return BadRequest(resposta);
                }
                
                await _verificacaoService.GerarCodigoVerificacao(email);
                resposta.Dados = true;
                resposta.Mensagem = "Código de verificação reenviado com sucesso!";
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro ao reenviar código de verificação: {ex.Message}";
                return BadRequest(resposta);
            }
        }
    }
}

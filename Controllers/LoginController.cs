using Microsoft.AspNetCore.Mvc;
using Dto.Aluno;
using Alunos.Services;
using Backend_Vestetec_App.Models;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult<ResponseModel<string>>> Login(LoginDto loginDto)
        {
            var resultado = await _loginService.Login(loginDto);

            if (!resultado.status)
                return Unauthorized(resultado);

            return Ok(resultado);
        }

        [HttpPost("alterar-senha")]
        public async Task<ActionResult<ResponseModel<bool>>> AlterarSenha(AlteracaoSenhaDto alteracaoSenhaDto)
        {
            var resultado = await _loginService.AlterarSenha(alteracaoSenhaDto);

            if (!resultado.status)
                return BadRequest(resultado);

            return Ok(resultado);
        }
    }
}
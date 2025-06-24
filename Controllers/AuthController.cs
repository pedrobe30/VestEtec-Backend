// Controllers/AuthController.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Vestetec_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realizar login do administrador
        /// </summary>
        /// <param name="loginDto">Dados de login (email e senha)</param>
        /// <returns>Dados do administrador logado</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginAdmResponseDto>> Login([FromBody] LoginAdmDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var loginResponse = await _authService.LoginAsync(loginDto);
                if (loginResponse == null)
                    return Unauthorized(new { message = "Credenciais inválidas" });

                return Ok(loginResponse);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Alterar senha do administrador
        /// </summary>
        /// <param name="alterarSenhaDto">Dados para alteração de senha</param>
        /// <returns>Resultado da operação</returns>
        [HttpPut("alterar-senha")]
        public async Task<ActionResult> AlterarSenha([FromBody] AlterarSenhaDto alterarSenhaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultado = await _authService.AlterarSenhaAsync(alterarSenhaDto);
                if (!resultado)
                    return BadRequest(new { message = "Não foi possível alterar a senha" });

                return Ok(new { message = "Senha alterada com sucesso" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Redefinir senha esquecida (requer código preciso)
        /// </summary>
        /// <param name="esqueciSenhaDto">Dados para redefinição de senha</param>
        /// <returns>Resultado da operação</returns>
        [HttpPut("esqueci-senha")]
        public async Task<ActionResult> EsqueciSenha([FromBody] EsqueciSenhaDto esqueciSenhaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var resultado = await _authService.EsqueciSenhaAsync(esqueciSenhaDto);
                if (!resultado)
                    return BadRequest(new { message = "Não foi possível redefinir a senha" });

                return Ok(new { message = "Senha redefinida com sucesso" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
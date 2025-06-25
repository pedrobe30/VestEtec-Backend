// Controllers/AuthController.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        /// <returns>Dados do administrador logado com token JWT</returns>
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
        /// Validar token JWT do administrador
        /// </summary>
        /// <param name="token">Token JWT para validação</param>
        /// <returns>Dados do token validado</returns>
        [HttpPost("validate-token")]
        public async Task<ActionResult<ValidateAdminTokenDto>> ValidateToken([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { message = "Token é obrigatório" });

                var result = await _authService.ValidateTokenAsync(token);
                if (result == null || !result.IsValid)
                    return Unauthorized(new { message = "Token inválido ou expirado" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Validar token JWT através do header Authorization
        /// </summary>
        /// <returns>Dados do token validado</returns>
        [HttpGet("validate-token")]
        public async Task<ActionResult<ValidateAdminTokenDto>> ValidateTokenFromHeader()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return BadRequest(new { message = "Header Authorization com Bearer token é obrigatório" });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var result = await _authService.ValidateTokenAsync(token);

                if (result == null || !result.IsValid)
                    return Unauthorized(new { message = "Token inválido ou expirado" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Renovar token JWT do administrador
        /// </summary>
        /// <param name="token">Token JWT atual para renovação</param>
        /// <returns>Novo token JWT</returns>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginAdmResponseDto>> RefreshToken([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { message = "Token é obrigatório" });

                var result = await _authService.RefreshTokenAsync(token);
                if (result == null)
                    return Unauthorized(new { message = "Token inválido ou expirado" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Renovar token JWT através do header Authorization
        /// </summary>
        /// <returns>Novo token JWT</returns>
        [HttpPost("refresh-token-header")]
        public async Task<ActionResult<LoginAdmResponseDto>> RefreshTokenFromHeader()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return BadRequest(new { message = "Header Authorization com Bearer token é obrigatório" });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var result = await _authService.RefreshTokenAsync(token);

                if (result == null)
                    return Unauthorized(new { message = "Token inválido ou expirado" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Logout - revogar token JWT
        /// </summary>
        /// <param name="token">Token JWT para revogar</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { message = "Token é obrigatório" });

                var result = await _authService.RevokeTokenAsync(token);
                if (!result)
                    return BadRequest(new { message = "Token inválido" });

                return Ok(new { message = "Logout realizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Logout através do header Authorization
        /// </summary>
        /// <returns>Resultado da operação</returns>
        [HttpPost("logout-header")]
        public async Task<ActionResult> LogoutFromHeader()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return BadRequest(new { message = "Header Authorization com Bearer token é obrigatório" });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var result = await _authService.RevokeTokenAsync(token);

                if (!result)
                    return BadRequest(new { message = "Token inválido" });

                return Ok(new { message = "Logout realizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obter informações do administrador logado através do token
        /// </summary>
        /// <returns>Dados do administrador</returns>
        [HttpGet("me")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ValidateAdminTokenDto>> GetCurrentAdmin()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return BadRequest(new { message = "Header Authorization com Bearer token é obrigatório" });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var result = await _authService.ValidateTokenAsync(token);

                if (result == null || !result.IsValid)
                    return Unauthorized(new { message = "Token inválido ou expirado" });

                return Ok(result);
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

        /// <summary>
        /// Verificar se o token é válido (endpoint simples)
        /// </summary>
        /// <param name="token">Token para verificação</param>
        /// <returns>Status da validação</returns>
        [HttpPost("check-token")]
        public ActionResult<object> CheckToken([FromBody] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { message = "Token é obrigatório" });

                var isValid = _authService.IsTokenValid(token);
                var adminId = _authService.GetAdminIdFromToken(token);
                var email = _authService.GetEmailFromToken(token);

                return Ok(new
                {
                    isValid = isValid,
                    adminId = adminId,
                    email = email,
                    message = isValid ? "Token válido" : "Token inválido"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
// Services/AuthService.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend_Vestetec_App.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const string CODIGO_PRECISO_VALIDO = "0309";
        private readonly HashSet<string> _revokedTokens = new();

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginAdmResponseDto?> LoginAsync(LoginAdmDto loginAdmDto)
        {
            try
            {
                // Buscar administrador pelo email
                var admin = await _context.Admins
                    .Include(a => a.IdEmpresaNavigation)
                    .FirstOrDefaultAsync(a => a.Email.ToLower() == loginAdmDto.Email.ToLower());

                if (admin == null)
                {
                    throw new ArgumentException("Email ou senha incorretos.");
                }

                // Verificar senha
                var senhaHash = HashPassword(loginAdmDto.Senha);
                if (admin.Senha != senhaHash)
                {
                    throw new ArgumentException("Email ou senha incorretos.");
                }

                // Gerar token JWT
                var token = GerarTokenJwtAdmin(admin);

                // Retornar dados do login com token
                return new LoginAdmResponseDto
                {
                    IdAdm = admin.IdAdm,
                    Email = admin.Email,
                    Nome = admin.Nome,
                    IdEmpresa = admin.IdEmpresa,
                    NomeEmpresa = admin.IdEmpresaNavigation?.Nome,
                    DataLogin = DateTime.Now,
                    Token = token,
                    Message = "Login realizado com sucesso"
                };
            }
            catch (ArgumentException)
            {
                throw; // Re-throw para manter a mensagem específica
            }
            catch (Exception ex)
            {
                throw new Exception("Erro interno no processo de login.", ex);
            }
        }

        public async Task<bool> AlterarSenhaAsync(AlterarSenhaDto alterarSenhaDto)
        {
            try
            {
                // Buscar administrador pelo email
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email.ToLower() == alterarSenhaDto.Email.ToLower());

                if (admin == null)
                {
                    throw new ArgumentException("Email não encontrado.");
                }

                // Verificar senha atual
                var senhaAtualHash = HashPassword(alterarSenhaDto.SenhaAtual);
                if (admin.Senha != senhaAtualHash)
                {
                    throw new ArgumentException("Senha atual incorreta.");
                }

                // Verificar se a nova senha é diferente da atual
                var novaSenhaHash = HashPassword(alterarSenhaDto.NovaSenha);
                if (admin.Senha == novaSenhaHash)
                {
                    throw new ArgumentException("A nova senha deve ser diferente da senha atual.");
                }

                // Atualizar senha
                admin.Senha = novaSenhaHash;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (ArgumentException)
            {
                throw; // Re-throw para manter a mensagem específica
            }
            catch (Exception ex)
            {
                throw new Exception("Erro interno no processo de alteração de senha.", ex);
            }
        }

        public async Task<bool> EsqueciSenhaAsync(EsqueciSenhaDto esqueciSenhaDto)
        {
            try
            {
                // Validar código preciso
                if (esqueciSenhaDto.CodigoPreciso != CODIGO_PRECISO_VALIDO)
                {
                    throw new ArgumentException("Código preciso inválido. Não é possível alterar a senha.");
                }

                // Buscar administrador pelo email
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email.ToLower() == esqueciSenhaDto.Email.ToLower());

                if (admin == null)
                {
                    throw new ArgumentException("Email não encontrado.");
                }

                // Verificar se a nova senha é diferente da atual
                var novaSenhaHash = HashPassword(esqueciSenhaDto.NovaSenha);
                if (admin.Senha == novaSenhaHash)
                {
                    throw new ArgumentException("A nova senha deve ser diferente da senha atual.");
                }

                // Atualizar senha
                admin.Senha = novaSenhaHash;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (ArgumentException)
            {
                throw; // Re-throw para manter a mensagem específica
            }
            catch (Exception ex)
            {
                throw new Exception("Erro interno no processo de recuperação de senha.", ex);
            }
        }

        public async Task<ValidateAdminTokenDto?> ValidateTokenAsync(string token)
        {
            try
            {
                // Verificar se o token foi revogado
                if (_revokedTokens.Contains(token))
                {
                    return new ValidateAdminTokenDto { IsValid = false };
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extrair informações do token
                var adminId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var nome = principal.FindFirst(ClaimTypes.Name)?.Value;
                var idEmpresa = int.Parse(principal.FindFirst("IdEmpresa")?.Value ?? "0");

                // Buscar dados atualizados do admin no banco
                var admin = await _context.Admins
                    .Include(a => a.IdEmpresaNavigation)
                    .FirstOrDefaultAsync(a => a.IdAdm == adminId);

                if (admin == null)
                {
                    return new ValidateAdminTokenDto { IsValid = false };
                }

                return new ValidateAdminTokenDto
                {
                    IdAdm = admin.IdAdm,
                    Email = admin.Email,
                    Nome = admin.Nome,
                    IdEmpresa = admin.IdEmpresa,
                    NomeEmpresa = admin.IdEmpresaNavigation?.Nome,
                    IsValid = true,
                    ExpiresAt = jwtToken.ValidTo
                };
            }
            catch
            {
                return new ValidateAdminTokenDto { IsValid = false };
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                // Validar se o token é válido antes de revogar
                if (!IsTokenValid(token))
                {
                    return false;
                }

                // Adicionar token à lista de tokens revogados
                _revokedTokens.Add(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginAdmResponseDto?> RefreshTokenAsync(string token)
        {
            try
            {
                var validateResult = await ValidateTokenAsync(token);
                if (validateResult == null || !validateResult.IsValid)
                {
                    return null;
                }

                // Buscar admin atualizado
                var admin = await _context.Admins
                    .Include(a => a.IdEmpresaNavigation)
                    .FirstOrDefaultAsync(a => a.IdAdm == validateResult.IdAdm);

                if (admin == null)
                {
                    return null;
                }

                // Revogar token antigo
                await RevokeTokenAsync(token);

                // Gerar novo token
                var newToken = GerarTokenJwtAdmin(admin);

                return new LoginAdmResponseDto
                {
                    IdAdm = admin.IdAdm,
                    Email = admin.Email,
                    Nome = admin.Nome,
                    IdEmpresa = admin.IdEmpresa,
                    NomeEmpresa = admin.IdEmpresaNavigation?.Nome,
                    DataLogin = DateTime.Now,
                    Token = newToken,
                    Message = "Token renovado com sucesso"
                };
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenValid(string token)
        {
            try
            {
                // Verificar se o token foi revogado
                if (_revokedTokens.Contains(token))
                {
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int? GetAdminIdFromToken(string token)
        {
            try
            {
                if (!IsTokenValid(token))
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var adminIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(adminIdClaim, out int adminId) ? adminId : null;
            }
            catch
            {
                return null;
            }
        }

        public string? GetEmailFromToken(string token)
        {
            try
            {
                if (!IsTokenValid(token))
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            }
            catch
            {
                return null;
            }
        }

        private string GerarTokenJwtAdmin(Admin admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.IdAdm.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Name, admin.Nome),
                new Claim("IdEmpresa", admin.IdEmpresa.ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = issuer,
                Audience = audience,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
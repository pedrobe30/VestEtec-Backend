using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Dto.Aluno;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using Backend_Vestetec_App.Services;


namespace Alunos.Services
{
    public class LoginService : ILoginInterface
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IVerificacaoService _verificacaoService;

        public LoginService(AppDbContext context, IConfiguration configuration, IVerificacaoService verificacaoService)
        {
            _verificacaoService = verificacaoService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<ResponseModel<string>> Login(LoginDto loginDto)
        {
            var resposta = new ResponseModel<string>();

            try
            {
                var aluno = await _context.Alunos
                    .FirstOrDefaultAsync(veri => veri.EmailAlu == loginDto.email);

                if (aluno == null)
                {
                    resposta.status = false;
                    resposta.Mensagem = "Aluno não encontrado";
                    return resposta;
                }

                if (!await _verificacaoService.EmailEstaVerificado(aluno.EmailAlu))
                {
                    resposta.status = false;
                    resposta.Mensagem = "Você não está verificado, Por favor verifique o codigo que chegou no seu email";
                    return resposta;
                }


                if (!BCrypt.Net.BCrypt.Verify(loginDto.senha, aluno.SenhaAlu))
                {
                    resposta.status = false;
                    resposta.Mensagem = "Senha incorreta";
                    return resposta;
                }


                var token = GerarTokenJwt(aluno);
                resposta.Dados = token;
                resposta.Mensagem = "Login realizado com sucesso";
                return resposta;
            }

            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro durante o Login{ex.Message}";
                Console.WriteLine($"Login error: {ex}");
                return resposta;
            }
        }

        public async Task<ResponseModel<bool>> AlterarSenha(AlteracaoSenhaDto alteracaoSenhaDto)
        {
            var resposta = new ResponseModel<bool>();

            try
            {
                var aluno = await _context.Alunos
                    .FirstOrDefaultAsync(veri => veri.EmailAlu == alteracaoSenhaDto.email);

                if (aluno == null)
                {
                    resposta.status = false;
                    resposta.Mensagem = "Aluno Não Encontrado";
                    return resposta;
                }

                if (!BCrypt.Net.BCrypt.Verify(alteracaoSenhaDto.senhaAtual, aluno.SenhaAlu))
                {
                    resposta.status = false;
                    resposta.Mensagem = "Senha Atual Incorreta";
                    return resposta;
                }

                string novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(
                    alteracaoSenhaDto.novaSenha,
                    workFactor: 14
                );

                aluno.SenhaAlu = novaSenhaHash;
                await _context.SaveChangesAsync();

                resposta.Dados = true;
                resposta.Mensagem = "Senha alterada com sucesso";
                return resposta;
            }

            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro ao alterar senha: {ex.Message}";
                Console.WriteLine($"Login error: {ex}");
                return resposta;
            }
        }

        private string GerarTokenJwt(Aluno aluno)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claimsIdentity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, aluno.IdAluno.ToString()),
        new Claim(ClaimTypes.Email, aluno.EmailAlu),
        new Claim(ClaimTypes.Name, aluno.NomeAlu),
        // Opcional: adicionar JTI para identificar unicamente este token
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
    }
}
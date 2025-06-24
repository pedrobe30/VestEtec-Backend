// Services/AuthService.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Backend_Vestetec_App.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private const string CODIGO_PRECISO_VALIDO = "0309";

        public AuthService(AppDbContext context)
        {
            _context = context;
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
                    throw new ArgumentException("Email não encontrado.");
                }

                // Verificar senha
                var senhaHash = HashPassword(loginAdmDto.Senha);
                if (admin.Senha != senhaHash)
                {
                    throw new ArgumentException("Senha incorreta.");
                }

                // Retornar dados do login
                return new LoginAdmResponseDto
                {
                    IdAdm = admin.IdAdm,
                    Email = admin.Email,
                    Nome = admin.Nome,
                    IdEmpresa = admin.IdEmpresa,
                    NomeEmpresa = admin.IdEmpresaNavigation?.Nome,
                    DataLogin = DateTime.Now,
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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
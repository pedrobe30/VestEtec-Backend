using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Services;
using Dto.Aluno;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Backend_Vestetec_App.Controllers;

namespace Backend_Vestetec_App.Services
{
    public class VerificacaoService : IVerificacaoService
    {
        private readonly AppDbContext _context;
        private readonly IEnviarEmail _emailService;

        public VerificacaoService(AppDbContext context, IEnviarEmail emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> GerarCodigoVerificacao(string email)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.EmailAlu == email);
            if (aluno == null)
            {
                throw new Exception("Email não Encontrado no Sistema.");
            }

            Random random = new Random();
            string codigo = random.Next(100000, 999999).ToString();


            var codigosAnteriores = await _context.CodigosVerificacao
                .Where(c => c.Email == email && c.Ativo)
                .ToListAsync();

            foreach (var codigoAntigo in codigosAnteriores)
            {
                codigoAntigo.Ativo = false;
            }

            var novoCodigoVerificado = new CodigoVerificacao
            {
                Email = email,
                Codigo = codigo,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10),
                Ativo = true
            };

            _context.CodigosVerificacao.Add(novoCodigoVerificado);
            await _context.SaveChangesAsync();

            await _emailService.EnviarCodigoVerificacao(email, codigo);

            return codigo;
        }

        public async Task<ResponseModel<bool>> VerificarCodigo(verificacaoEmailDto verificacao)
        {
            ResponseModel<bool> resposta = new ResponseModel<bool>();

            try
            {
                // Buscar o código ativo mais recente para o email
                var codigoVerificacao = await _context.CodigosVerificacao
                    .Where(c => c.Email == verificacao.Email && c.Ativo)
                    .OrderByDescending(c => c.DataExpiracao)
                    .FirstOrDefaultAsync();

                // Verificar se existe um código ativo para este email
                if (codigoVerificacao == null)
                {
                    resposta.status = false;
                    resposta.Mensagem = "Nenhum código de verificação ativo foi encontrado para este email.";
                    return resposta;
                }

                // Verificar se o código não expirou
                if (DateTime.UtcNow > codigoVerificacao.DataExpiracao)
                {
                    codigoVerificacao.Ativo = false;
                    await _context.SaveChangesAsync();

                    resposta.status = false;
                    resposta.Mensagem = "O código de verificação expirou. Solicite um novo código.";
                    return resposta;
                }

                // Verificar se o código está correto
                if (codigoVerificacao.Codigo != verificacao.Codigo)
                {
                    resposta.status = false;
                    resposta.Mensagem = "Código de verificação incorreto.";
                    return resposta;
                }

                // Marcar o email como verificado
                var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.EmailAlu == verificacao.Email);
                if (aluno != null)
                {
                    aluno.EmailVerificado = true;

                    // Desativar o código usado
                    codigoVerificacao.Ativo = false;

                    await _context.SaveChangesAsync();

                    resposta.Dados = true;
                    resposta.Mensagem = "Email verificado com sucesso!";
                    return resposta;
                }
                else
                {
                    resposta.status = false;
                    resposta.Mensagem = "Aluno não encontrado.";
                    return resposta;
                }
            }
            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro ao verificar o código: {ex.Message}";
                return resposta;
            }
        }

        public async Task<bool> EmailEstaVerificado(string email)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.EmailAlu == email);
            return aluno != null && aluno.EmailVerificado;
        }

        public async Task<ResponseModel<bool>> MarcarEmailComoVerificado(string email)
        {
            ResponseModel<bool> resposta = new ResponseModel<bool>();

            try
            {
                var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.EmailAlu == email);

                if (aluno == null)
                {
                    resposta.status = false;
                    resposta.Mensagem = "Aluno não encontrado.";
                    return resposta;
                }

                aluno.EmailVerificado = true;
                await _context.SaveChangesAsync();

                resposta.Dados = true;
                resposta.Mensagem = "Email marcado como verificado.";
                return resposta;
            }
            catch (Exception ex)
            {
                resposta.status = false;
                resposta.Mensagem = $"Erro ao marcar email como verificado: {ex.Message}";
                return resposta;
            }
        }
    }
}

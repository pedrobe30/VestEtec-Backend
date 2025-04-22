
using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Services;
using Dto.Aluno;
using Microsoft.EntityFrameworkCore;



namespace Alunos.Services
{
    public class AlunoService : IAlunoInterface
    {
        private readonly AppDbContext _context;
        private readonly IEnviarEmail _emailService;
        private readonly IVerificacaoService _verificacaoService;
         public AlunoService(AppDbContext context, IEnviarEmail emailService, IVerificacaoService verificacaoService)
            {
                _context = context;
                _emailService = emailService;
                _verificacaoService = verificacaoService;
            }



        public async Task<ResponseModel<Aluno>> AdicionarAluno(CriacaoAluno criacaoAluno)
        {
            ResponseModel<Aluno> resposta = new ResponseModel<Aluno>();

            try
            {
                if (await _context.Alunos.AnyAsync(a => a.EmailAlu == criacaoAluno.EmailAlu))
                {
                    resposta.Mensagem = "Já existe uma conta utilizando esse Email";
                    resposta.status = false;
                    return resposta;
                }

                string hashSenha = BCrypt.Net.BCrypt.HashPassword(criacaoAluno.SenhaAlu, workFactor: 14);

                var aluno = new Aluno()
                {
                    NomeAlu = criacaoAluno.NomeAlu,
                    Rm = criacaoAluno.Rm,
                    EmailAlu = criacaoAluno.EmailAlu,
                    SenhaAlu = hashSenha,
                    IdEsc = criacaoAluno.IdEsc
                };

                _context.Add(aluno);
                await _context.SaveChangesAsync();

                await _verificacaoService.GerarCodigoVerificacao(aluno.EmailAlu);

                aluno.SenhaAlu = null!;

                resposta.Dados = aluno;
                resposta.Mensagem = "Cadastrado com sucesso! Verifique o Codigo de Verificação enviado ao seu email para prosseguir com o login";

                return resposta;
            }
            catch (Exception ex)
            {
                resposta.Mensagem = ex.Message;

                resposta.status = false;

                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine("Erro ao salvar no banco: " + errorMessage);

                return resposta;
            }
        }

        public async Task<ResponseModel<Aluno>> AtualizarSenha(int id, string novaSenha)
        {
            ResponseModel<Aluno> resposta = new ResponseModel<Aluno>();

            try
            {
                var aluno = await _context.Alunos.FindAsync(id);

                if (aluno == null)
                {
                    resposta.Mensagem = "Aluno não encontrado";
                    resposta.status = false;
                    return resposta;
                }

                string hashSenha = BCrypt.Net.BCrypt.HashPassword(novaSenha, workFactor: 14);
                aluno.SenhaAlu = hashSenha;

                await _context.SaveChangesAsync();

                // Não retornar a senha no objeto de resposta
                var alunoRetorno = new Aluno
                {
                    IdAluno = aluno.IdAluno,
                    NomeAlu = aluno.NomeAlu,
                    EmailAlu = aluno.EmailAlu,
                    Rm = aluno.Rm,
                    IdEsc = aluno.IdEsc
                };

                resposta.Dados = alunoRetorno;
                resposta.Mensagem = "Senha atualizada com sucesso";

                return resposta;
            }
            catch (Exception ex)
            {
                resposta.Mensagem = $"Erro ao atualizar senha: {ex.Message}";
                resposta.status = false;
                return resposta;
            }

        }

    
    }
}
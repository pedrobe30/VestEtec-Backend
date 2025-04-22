using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Dto.Aluno;
using Backend_Vestetec_App.Models;

namespace Alunos.Services
{

    public interface ILoginInterface
    {
          Task<ResponseModel<string>> Login(LoginDto loginDto);
        Task<ResponseModel<bool>> AlterarSenha(AlteracaoSenhaDto alteracaoSenhaDto);
    }
}
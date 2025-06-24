// Interfaces/IAuthService.cs
using Backend_Vestetec_App.DTOs;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IAuthService
    {
        Task<LoginAdmResponseDto?> LoginAsync(LoginAdmDto loginAdmDto);
        Task<bool> AlterarSenhaAsync(AlterarSenhaDto alterarSenhaDto);
        Task<bool> EsqueciSenhaAsync(EsqueciSenhaDto esqueciSenhaDto);
    }
}
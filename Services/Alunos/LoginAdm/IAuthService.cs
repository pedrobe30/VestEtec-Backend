// Interfaces/IAuthService.cs
using Backend_Vestetec_App.DTOs;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IAuthService
    {
        Task<LoginAdmResponseDto?> LoginAsync(LoginAdmDto loginAdmDto);
        Task<bool> AlterarSenhaAsync(AlterarSenhaDto alterarSenhaDto);
        Task<bool> EsqueciSenhaAsync(EsqueciSenhaDto esqueciSenhaDto);
        Task<ValidateAdminTokenDto?> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<LoginAdmResponseDto?> RefreshTokenAsync(string token);
        bool IsTokenValid(string token);
        int? GetAdminIdFromToken(string token);
        string? GetEmailFromToken(string token);
    }
}
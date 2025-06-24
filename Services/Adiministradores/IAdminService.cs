// Interfaces/IAdminService.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Models;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IAdminService
    {
        Task<AdminResponseDto> CreateAdminAsync(AdminCreateDto adminDto);
        Task<AdminResponseDto?> GetAdminByIdAsync(int id);
        Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync();
        Task<AdminResponseDto?> UpdateAdminAsync(int id, AdminUpdateDto adminDto);
        Task<bool> DeleteAdminAsync(int id);
        Task<bool> AdminExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    }
}
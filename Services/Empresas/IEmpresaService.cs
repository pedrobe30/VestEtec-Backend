using Backend_Vestetec_App.DTOs;

namespace Backend_Vestetec_App.Interfaces;

public interface IEmpresaService
{
    Task<IEnumerable<EmpresaDto>> GetAllAsync();
    Task<EmpresaDto?> GetByIdAsync(int id);
    Task<EmpresaDto> CreateAsync(EmpresaCreateDto empresaCreateDto);
    Task<EmpresaDto?> UpdateAsync(int id, EmpresaUpdateDto empresaUpdateDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<EmpresaDropdownDto>> GetForDropdownAsync();
    Task<bool> ExistsAsync(int id);
}
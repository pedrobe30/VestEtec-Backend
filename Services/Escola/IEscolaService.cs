using System.Collections.Generic;
using System.Threading.Tasks;
using Dto.Escola;

namespace Backend_Vestetec_App.Services
{
    public interface IEscolaService
    {
        Task<IEnumerable<EscolaDto>> GetAllEscolasAsync();
        Task<EscolaDto> GetEscolaByIdAsync(int id);
        // Add other methods as needed
    }
}
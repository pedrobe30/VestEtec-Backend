using Backend_Vestetec_App.Models;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IModeloService
    {
        Task<Modelo> CreateModelo(Modelo modelo);

        Task<IEnumerable<Modelo>> GetAllModelosAsync();

        Task<Modelo> UpdateModeloAsync(int id, Modelo modelo);

        Task<bool> DeleteModelo(int id);

        Task<bool> ModeloExists(int id);
    }
    
}
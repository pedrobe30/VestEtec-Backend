

using Backend_Vestetec_App.Models;

namespace Backend_Vestetec_App.Interfaces
 {
    public interface ITecidoService
    {
        Task<Tecido> CreateTecido(Tecido tecido);
        
        Task<IEnumerable<Tecido>> GetAllTecido();

        Task<Tecido> UpdateTecido(int id, Tecido tecido);

        Task<bool> DeleteTecido(int id);

        Task<bool> TecidoExists(int id);

    }
  
 }
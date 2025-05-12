using Backend_Vestetec_App.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Interfaces
{
    public interface ICategoriaService
    {
        // Retorna todas as categorias
        Task<IEnumerable<Categoria>> GetAllCategoriasAsync();
        
        // Busca categoria por ID
        Task<Categoria> GetCategoriaByIdAsync(int id);
        
        // Cria uma nova categoria
        Task<Categoria> CreateCategoriaAsync(Categoria categoria);
        
        // Atualiza uma categoria existente
        Task<Categoria> UpdateCategoriaAsync(int id, Categoria categoria);
        
        // Remove uma categoria
        Task<bool> DeleteCategoriaAsync(int id);
        
        // Verifica se uma categoria existe
        Task<bool> CategoriaExistsAsync(int id);
    }
}
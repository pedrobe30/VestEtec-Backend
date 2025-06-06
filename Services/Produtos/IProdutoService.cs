
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Models;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IProdutoService
    {
        Task<ResponseModel<List<ProdutoDto>>> GetAllProdutosAsync();
        Task<ResponseModel<ProdutoDto>> GetProdutoByIdAsync(int id);
        Task<ResponseModel<ProdutoDto>> AddProdutoCompletoAsync(ProdutoDto produtoDto);
        Task<ResponseModel<ProdutoDto>> UpdateProdutoAsync(int id, ProdutoDto produtoDto);
        Task<ResponseModel<bool>> DeleteProdutoAsync(int id);
        Task<ResponseModel<List<ProdutoDto>>> GetProdutosByCategoriaAsync(int categoriaId);

       
    }
}
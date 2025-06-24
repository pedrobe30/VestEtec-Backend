// Interfaces/IProdutoService.cs - CORRIGIDO
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Controllers;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IProdutoService
    {
        Task<ResponseModel<List<ProdutoResponseDto>>> GetAllProdutosAsync();

        Task<ResponseModel<ProdutoResponseDto>> GetProdutoByIdAsync(int id);

        Task<ResponseModel<List<ProdutoResponseDto>>> GetProdutosByCategoriaAsync(int categoriaId);

        // CORRIGIDO: Usar ProdutoCompletoDto (definido no controller)
        Task<ResponseModel<ProdutoResponseDto>> AddProdutoCompletoAsync(ProdutoCompletoDto produtoDto);

        // CORRIGIDO: Usar ProdutoUpdateCompletoDto (definido no controller)
        Task<ResponseModel<ProdutoResponseDto>> UpdateProdutoAsync(int id, ProdutoUpdateCompletoDto produtoDto);

        Task<ResponseModel<bool>> DeleteProdutoAsync(int id);
    }
}
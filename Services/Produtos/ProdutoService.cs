// Services/ProdutoService.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _context;

        public ProdutoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseModel<List<ProdutoDto>>> GetAllProdutosAsync()
        {
            var response = new ResponseModel<List<ProdutoDto>>();

            try
            {
                var produtos = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Select(p => new ProdutoDto
                    {
                        IdProd = p.IdProd,
                        Preco = p.Preco,
                        QuantEstoque = p.QuantEstoque,
                        IdCategoria = p.IdCategoria,
                        IdModelo = p.IdModelo,
                        IdTecido = p.IdTecido,
                        IdStatus = p.IdStatus,
                        ImgUrl = p.ImgUrl,
                        Descricao = p.descricao,
                        CategoriaNome = p.IdCategoriaNavigation.Categoria1, // Assuming the Category model has a Nome property
                        ModeloNome = p.IdModeloNavigation.Modelo1, // Assuming the Model model has a Nome property
                        TecidoNome = p.IdTecido.HasValue ? p.IdTecidoNavigation.Tipo : null, // Assuming the Tecido model has a Nome property
                        StatusNome = p.IdStatusNavigation.Descricao // Assuming the Status model has a Nome property
                    })
                    .ToListAsync();

                response.Dados = produtos;
                response.Mensagem = "Produtos recuperados com sucesso";
            }
            catch (Exception ex)
            {
                response.status = false;
                response.Mensagem = $"Erro ao recuperar produtos: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<ProdutoDto>> GetProdutoByIdAsync(int id)
        {
            var response = new ResponseModel<ProdutoDto>();

            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .FirstOrDefaultAsync(p => p.IdProd == id);

                if (produto == null)
                {
                    response.status = false;
                    response.Mensagem = "Produto não encontrado";
                    return response;
                }

                response.Dados = new ProdutoDto
                {
                    IdProd = produto.IdProd,
                    Preco = produto.Preco,
                    QuantEstoque = produto.QuantEstoque,
                    IdCategoria = produto.IdCategoria,
                    IdModelo = produto.IdModelo,
                    IdTecido = produto.IdTecido,
                    IdStatus = produto.IdStatus,
                    ImgUrl = produto.ImgUrl,
                    Descricao = produto.descricao,
                    CategoriaNome = produto.IdCategoriaNavigation.Categoria1,
                    ModeloNome = produto.IdModeloNavigation.Modelo1,
                    TecidoNome = produto.IdTecido.HasValue ? produto.IdTecidoNavigation.Tipo : null,
                    StatusNome = produto.IdStatusNavigation.Descricao
                };

                response.Mensagem = "Produto recuperado com sucesso";
            }
            catch (Exception ex)
            {
                response.status = false;
                response.Mensagem = $"Erro ao recuperar produto: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<ProdutoDto>> AddProdutoCompletoAsync(ProdutoCompletoDto produtoDto)
        {
            var response = new ResponseModel<ProdutoDto>();

            try
            {
                // Validate that the provided IDs exist
                var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == produtoDto.IdCategoria);
                var modeloExists = await _context.Modelos.AnyAsync(m => m.IdModelo == produtoDto.IdModelo);
                var tecidoExists = produtoDto.IdTecido.HasValue ?
                    await _context.Tecidos.AnyAsync(t => t.IdTecido == produtoDto.IdTecido) : true;

                if (!categoriaExists || !modeloExists || !tecidoExists)
                {
                    response.status = false;
                    response.Mensagem = "Uma ou mais referências (Categoria, Modelo, Tecido) não existem no banco de dados";
                    return response;
                }

                // Get default status (e.g., "Available")
                var defaultStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.Descricao == "Disponível");

                if (defaultStatus == null)
                {
                    response.status = false;
                    response.Mensagem = "Status padrão não encontrado";
                    return response;
                }

                var novoProduto = new Produto
                {
                    Preco = produtoDto.Preco,
                    descricao = produtoDto.Descricao,
                    QuantEstoque = produtoDto.QuantEstoque,
                    IdCategoria = produtoDto.IdCategoria,
                    IdModelo = produtoDto.IdModelo,
                    IdTecido = produtoDto.IdTecido,
                    IdStatus = defaultStatus.IdStatus,
                    ImgUrl = produtoDto.ImgUrl
                };

                _context.Produtos.Add(novoProduto);
                await _context.SaveChangesAsync();

                // Reload the product with navigation properties
                await _context.Entry(novoProduto).Reference(p => p.IdCategoriaNavigation).LoadAsync();
                await _context.Entry(novoProduto).Reference(p => p.IdModeloNavigation).LoadAsync();
                await _context.Entry(novoProduto).Reference(p => p.IdStatusNavigation).LoadAsync();
                if (novoProduto.IdTecido.HasValue)
                    await _context.Entry(novoProduto).Reference(p => p.IdTecidoNavigation).LoadAsync();

                response.Dados = new ProdutoDto
                {
                    IdProd = novoProduto.IdProd,
                    Preco = novoProduto.Preco,
                    QuantEstoque = novoProduto.QuantEstoque,
                    IdCategoria = novoProduto.IdCategoria,
                    IdModelo = novoProduto.IdModelo,
                    IdTecido = novoProduto.IdTecido,
                    IdStatus = novoProduto.IdStatus,
                    ImgUrl = novoProduto.ImgUrl,
                    Descricao = novoProduto.descricao,
                    CategoriaNome = novoProduto.IdCategoriaNavigation.Categoria1,
                    ModeloNome = novoProduto.IdModeloNavigation.Modelo1,
                    TecidoNome = novoProduto.IdTecido.HasValue ? novoProduto.IdTecidoNavigation.Tipo : null,
                    StatusNome = novoProduto.IdStatusNavigation.Descricao
                };

                response.Mensagem = "Produto adicionado com sucesso";
            }
            catch (Exception ex)
            {
                response.status = false;
                response.Mensagem = $"Erro ao adicionar produto: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<ProdutoDto>> UpdateProdutoAsync(int id, ProdutoDto produtoDto)
        {
            var response = new ResponseModel<ProdutoDto>();

            try
            {
                var produto = await _context.Produtos.FindAsync(id);

                if (produto == null)
                {
                    response.status = false;
                    response.Mensagem = "Produto não encontrado";
                    return response;
                }

                // Check if the referenced entities exist
                var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == produtoDto.IdCategoria);
                var modeloExists = await _context.Modelos.AnyAsync(m => m.IdModelo == produtoDto.IdModelo);
                var statusExists = await _context.Statuses.AnyAsync(s => s.IdStatus == produtoDto.IdStatus);
                var tecidoExists = produtoDto.IdTecido.HasValue ?
                    await _context.Tecidos.AnyAsync(t => t.IdTecido == produtoDto.IdTecido) : true;

                if (!categoriaExists || !modeloExists || !statusExists || !tecidoExists)
                {
                    response.status = false;
                    response.Mensagem = "Uma ou mais referências (Categoria, Modelo, Status, Tecido) não existem no banco de dados";
                    return response;
                }

                // Update product properties
                produto.Preco = produtoDto.Preco;
                produto.QuantEstoque = produtoDto.QuantEstoque;
                produto.IdCategoria = produtoDto.IdCategoria;
                produto.IdModelo = produtoDto.IdModelo;
                produto.IdTecido = produtoDto.IdTecido;
                produto.IdStatus = produtoDto.IdStatus;
                produto.ImgUrl = produtoDto.ImgUrl;
                produto.descricao = produtoDto.Descricao;

                _context.Produtos.Update(produto);
                await _context.SaveChangesAsync();

                // Reload the product with navigation properties
                await _context.Entry(produto).Reference(p => p.IdCategoriaNavigation).LoadAsync();
                await _context.Entry(produto).Reference(p => p.IdModeloNavigation).LoadAsync();
                await _context.Entry(produto).Reference(p => p.IdStatusNavigation).LoadAsync();
                if (produto.IdTecido.HasValue)
                    await _context.Entry(produto).Reference(p => p.IdTecidoNavigation).LoadAsync();

                response.Dados = new ProdutoDto
                {
                    IdProd = produto.IdProd,
                    Preco = produto.Preco,
                    QuantEstoque = produto.QuantEstoque,
                    IdCategoria = produto.IdCategoria,
                    IdModelo = produto.IdModelo,
                    IdTecido = produto.IdTecido,
                    IdStatus = produto.IdStatus,
                    ImgUrl = produto.ImgUrl,
                    Descricao = produto.descricao,
                    CategoriaNome = produto.IdCategoriaNavigation.Categoria1,
                    ModeloNome = produto.IdModeloNavigation.Modelo1,
                    TecidoNome = produto.IdTecido.HasValue ? produto.IdTecidoNavigation.Tipo : null,
                    StatusNome = produto.IdStatusNavigation.Descricao
                };

                response.Mensagem = "Produto atualizado com sucesso";
            }
            catch (Exception ex)
            {
                response.status = false;
                response.Mensagem = $"Erro ao atualizar produto: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<bool>> DeleteProdutoAsync(int id)
        {
            var response = new ResponseModel<bool>();

            try
            {
                var produto = await _context.Produtos.FindAsync(id);

                if (produto == null)
                {
                    response.status = false;
                    response.Mensagem = "Produto não encontrado";
                    response.Dados = false;
                    return response;
                }

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                response.Dados = true;
                response.Mensagem = "Produto excluído com sucesso";
            }
            catch (Exception ex)
            {
                response.status = false;
                response.Mensagem = $"Erro ao excluir produto: {ex.Message}";
                response.Dados = false;
            }

            return response;
        }
    }
}
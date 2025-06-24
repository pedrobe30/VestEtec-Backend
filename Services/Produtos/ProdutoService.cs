// Services/ProdutoService.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;
        private readonly ILogger<ProdutoService> _logger;
        private const int STATUS_DISPONIVEL = 1;
        private const int STATUS_INDISPONIVEL = 2;

        public ProdutoService(AppDbContext context, IImageService imageService, ILogger<ProdutoService> logger)
        {
            _context = context;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<ResponseModel<List<ProdutoResponseDto>>> GetAllProdutosAsync()
        {
            var response = new ResponseModel<List<ProdutoResponseDto>>();
            try
            {
                var produtos = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .AsNoTracking()
                    .ToListAsync();

                response.Dados = produtos.Select(CreateProdutoResponseDto).ToList();
                response.Mensagem = "Produtos recuperados com sucesso.";
                response.status = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todos os produtos.");
                response.status = false;
                response.Mensagem = "Erro interno ao buscar produtos.";
            }
            return response;
        }

        public async Task<ResponseModel<ProdutoResponseDto>> GetProdutoByIdAsync(int id)
        {
            var response = new ResponseModel<ProdutoResponseDto>();
            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdProd == id);

                if (produto == null)
                {
                    response.status = false;
                    response.Mensagem = "Produto não encontrado";
                    return response;
                }

                response.Dados = CreateProdutoResponseDto(produto);
                response.status = true;
                response.Mensagem = "Produto recuperado com sucesso.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar produto por ID {ProdutoId}", id);
                response.status = false;
                response.Mensagem = "Erro interno ao buscar o produto.";
            }
            return response;
        }

        public async Task<ResponseModel<List<ProdutoResponseDto>>> GetProdutosByCategoriaAsync(int categoriaId)
        {
            var response = new ResponseModel<List<ProdutoResponseDto>>();
            try
            {
                var produtos = await _context.Produtos
                    .Where(p => p.IdCategoria == categoriaId)
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .AsNoTracking()
                    .ToListAsync();

                response.Dados = produtos.Select(CreateProdutoResponseDto).ToList();
                response.status = true;
                response.Mensagem = "Produtos recuperados com sucesso.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos pela categoria {CategoriaId}", categoriaId);
                response.status = false;
                response.Mensagem = "Erro interno ao buscar produtos por categoria.";
            }
            return response;
        }

        public async Task<ResponseModel<ProdutoResponseDto>> AddProdutoCompletoAsync(ProdutoCompletoDto produtoDto)
        {
            var response = new ResponseModel<ProdutoResponseDto>();

            // Validações iniciais...
            if (produtoDto.Imagem == null || !_imageService.IsValidImageFile(produtoDto.Imagem))
            {
                response.status = false;
                response.Mensagem = "Imagem é obrigatória e deve ser um formato válido.";
                return response;
            }
            if (!await ValidarReferenciasAsync(produtoDto.IdCategoria, produtoDto.IdModelo, produtoDto.IdTecido))
            {
                response.status = false;
                response.Mensagem = "Uma ou mais referências (categoria, modelo, tecido) não foram encontradas.";
                return response;
            }

            string? imagePath = null;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Salvar imagem
                imagePath = await _imageService.SaveImageAsync(produtoDto.Imagem);

                // 1) Criar e inserir produto
                var novoProduto = new Produto
                {
                    Preco = produtoDto.Preco,
                    IdCategoria = produtoDto.IdCategoria,
                    IdModelo = produtoDto.IdModelo,
                    IdTecido = produtoDto.IdTecido,
                    IdStatus = STATUS_DISPONIVEL,
                    ImgUrl = imagePath,
                    descricao = produtoDto.Descricao
                };
                _context.Produtos.Add(novoProduto);
                await _context.SaveChangesAsync();

                // Log do ID gerado
                _logger.LogInformation("Produto criado com ID: {ProdutoId}", novoProduto.IdProd);
                if (novoProduto.IdProd <= 0)
                {
                    throw new InvalidOperationException("ID do produto não foi gerado corretamente.");
                }

                _logger.LogInformation("Generated Produto.IdProd = {IdProd}", novoProduto.IdProd);
                if (novoProduto.IdProd <= 0)
                    throw new InvalidOperationException("ID do produto não foi gerado corretamente.");

                // 2) Inserir estoques
                if (produtoDto.TamanhosQuantidades != null && produtoDto.TamanhosQuantidades.Any())
                {
                    var tamanhosValidos = produtoDto.TamanhosQuantidades
                        .Where(tq => !string.IsNullOrWhiteSpace(tq.Tamanho))
                        .ToList();

                    if (tamanhosValidos.Any())
                    {
                        var estoques = tamanhosValidos
                            .Select(tq => new Estoque
                            {
                                IdProduto = novoProduto.IdProd,
                                Tamanho = tq.Tamanho.Trim().ToUpper(),
                                Quantidade = Math.Max(0, tq.Quantidade)
                            })
                            .GroupBy(e => e.Tamanho)
                            .Select(g => new Estoque
                            {
                                IdProduto = novoProduto.IdProd,
                                Tamanho = g.Key,
                                Quantidade = g.Sum(e => e.Quantidade)
                            })
                            .ToList();

                        foreach (var est in estoques)
                        {
                            _logger.LogInformation("Inserindo Estoque: ProdutoID={ProdutoId}, Tamanho='{Tamanho}', Quantidade={Quantidade}",
                                novoProduto.IdProd, est.Tamanho, est.Quantidade);
                        }

                        try
                        {
                            await _context.Estoques.AddRangeAsync(estoques);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Estoques inseridos com sucesso para ProdutoID={ProdutoId}", novoProduto.IdProd);
                        }
                        catch (Exception estEx)
                        {
                            var fkMsg = estEx.InnerException?.Message ?? estEx.Message;
                            _logger.LogError(estEx, "Erro ao salvar estoque para ProdutoID={ProdutoId}. {Msg}", novoProduto.IdProd, fkMsg);
                            throw new InvalidOperationException($"Erro ao salvar estoque: {fkMsg}");
                        }
                    }
                }

                await transaction.CommitAsync();

                // 3) Recuperar produto completo para retornar
                var produtoCriado = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdProd == novoProduto.IdProd);

                if (produtoCriado == null)
                    throw new InvalidOperationException($"Produto criado mas não encontrado com ID {novoProduto.IdProd}");

                response.Dados = CreateProdutoResponseDto(produtoCriado);
                response.status = true;
                response.Mensagem = "Produto criado com sucesso.";
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    try { _imageService.DeleteImage(imagePath); }
                    catch { /* ignora */ }
                }
                var detalhe = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "❌ ERRO DE BANCO DE DADOS ao criar produto. {Detalle}", detalhe);
                response.status = false;
                response.Mensagem = $"Erro interno ao criar produto: {ex.Message}. Detalhes: {detalhe}";
                return response;
            }
        }



        public async Task<ResponseModel<ProdutoResponseDto>> UpdateProdutoAsync(int id, ProdutoUpdateCompletoDto produtoDto)
        {
            var response = new ResponseModel<ProdutoResponseDto>();

            // Validar se o produto existe
            var produtoExistente = await _context.Produtos
                .Include(p => p.Estoque)
                .FirstOrDefaultAsync(p => p.IdProd == id);

            if (produtoExistente == null)
            {
                response.status = false;
                response.Mensagem = "Produto não encontrado.";
                return response;
            }

            // Validar se categoria, modelo e tecido existem
            if (!await ValidarReferenciasAsync(produtoDto.IdCategoria, produtoDto.IdModelo, produtoDto.IdTecido))
            {
                response.status = false;
                response.Mensagem = "Uma ou mais referências (categoria, modelo, tecido) não foram encontradas.";
                return response;
            }

            // Validar status
            if (produtoDto.IdStatus != STATUS_DISPONIVEL && produtoDto.IdStatus != STATUS_INDISPONIVEL)
            {
                response.status = false;
                response.Mensagem = "Status deve ser 1 (Disponível) ou 2 (Indisponível).";
                return response;
            }

            string? novaImagemPath = null;
            string? imagemAntigaPath = produtoExistente.ImgUrl;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Processar nova imagem se fornecida
                if (produtoDto.Imagem != null)
                {
                    if (!_imageService.IsValidImageFile(produtoDto.Imagem))
                    {
                        response.status = false;
                        response.Mensagem = "Formato de imagem inválido.";
                        return response;
                    }

                    novaImagemPath = await _imageService.SaveImageAsync(produtoDto.Imagem);
                }

                // Atualizar dados do produto
                produtoExistente.Preco = produtoDto.Preco;
                produtoExistente.IdCategoria = produtoDto.IdCategoria;
                produtoExistente.IdModelo = produtoDto.IdModelo;
                produtoExistente.IdTecido = produtoDto.IdTecido;
                produtoExistente.IdStatus = produtoDto.IdStatus;
                produtoExistente.descricao = produtoDto.Descricao;

                // Atualizar imagem se uma nova foi fornecida
                if (!string.IsNullOrWhiteSpace(novaImagemPath))
                {
                    produtoExistente.ImgUrl = novaImagemPath;
                }

                // Remover estoques existentes
                var estoquesExistentes = await _context.Estoques
                    .Where(e => e.IdProduto == id)
                    .ToListAsync();

                if (estoquesExistentes.Any())
                {
                    _context.Estoques.RemoveRange(estoquesExistentes);
                    _logger.LogInformation("Removidos {Count} estoques existentes do produto {ProdutoId}", estoquesExistentes.Count, id);
                    await _context.SaveChangesAsync();
                }

                // Adicionar novos estoques
                if (produtoDto.TamanhosQuantidades != null && produtoDto.TamanhosQuantidades.Any())
                {
                    // Logar recebimento
                    foreach (var tq in produtoDto.TamanhosQuantidades)
                    {
                        _logger.LogInformation("Novo item de estoque recebido para atualização: Tamanho='{Tamanho}', Quantidade={Quantidade}", tq.Tamanho, tq.Quantidade);
                    }

                    var tamanhosValidos = produtoDto.TamanhosQuantidades
                        .Where(tq => !string.IsNullOrWhiteSpace(tq.Tamanho))
                        .ToList();

                    if (tamanhosValidos.Any())
                    {
                        var novosEstoques = tamanhosValidos
                            .Select(tq => new Estoque
                            {
                                IdProduto = id,
                                Tamanho = tq.Tamanho.Trim().ToUpper(),
                                Quantidade = Math.Max(0, tq.Quantidade)
                            })
                            .GroupBy(e => e.Tamanho)
                            .Select(g => new Estoque
                            {
                                IdProduto = id,
                                Tamanho = g.Key,
                                Quantidade = g.Sum(e => e.Quantidade)
                            })
                            .ToList();

                        // Log dos estoques a inserir
                        foreach (var est in novosEstoques)
                        {
                            _logger.LogInformation("Inserindo novo Estoque para atualização: ProdutoID={ProdutoId}, Tamanho='{Tamanho}', Quantidade={Quantidade}",
                                id, est.Tamanho, est.Quantidade);
                        }

                        try
                        {
                            await _context.Estoques.AddRangeAsync(novosEstoques);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Estoques atualizados: {Count} itens para Produto {ProdutoId}", novosEstoques.Count, id);
                        }
                        catch (Exception estEx)
                        {
                            var innerMsg = estEx.InnerException?.Message ?? estEx.Message;
                            _logger.LogError(estEx, "Erro ao salvar novos estoques para Produto {ProdutoId}. InnerException: {Inner}", id, innerMsg);
                            throw new InvalidOperationException($"Erro ao salvar estoque na atualização: {innerMsg}");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Deletar imagem antiga se uma nova foi salva
                if (!string.IsNullOrWhiteSpace(novaImagemPath) && !string.IsNullOrWhiteSpace(imagemAntigaPath))
                {
                    try
                    {
                        _imageService.DeleteImage(imagemAntigaPath);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Erro ao deletar imagem antiga durante atualização");
                    }
                }

                // Buscar produto atualizado
                var produtoAtualizado = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdProd == id);

                if (produtoAtualizado == null)
                {
                    throw new InvalidOperationException($"Produto com ID {id} não foi encontrado após atualização");
                }

                response.Dados = CreateProdutoResponseDto(produtoAtualizado);
                response.status = true;
                response.Mensagem = "Produto atualizado com sucesso.";

                _logger.LogInformation("Produto {ProdutoId} atualizado com sucesso", id);

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Tentar deletar nova imagem se foi salva
                if (!string.IsNullOrWhiteSpace(novaImagemPath))
                {
                    try
                    {
                        _imageService.DeleteImage(novaImagemPath);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Erro ao deletar nova imagem durante rollback");
                    }
                }

                var inner = ex.InnerException?.Message;
                _logger.LogError(ex, "Erro ao atualizar produto {ProdutoId}: {Message}. InnerException: {Inner}", id, ex.Message, inner);
                response.status = false;
                response.Mensagem = $"Erro interno ao atualizar produto: {ex.Message}. Detalhes: {inner}";
                return response;
            }
        }

        public async Task<ResponseModel<bool>> DeleteProdutoAsync(int id)
        {
            var response = new ResponseModel<bool>();

            try
            {
                var produto = await _context.Produtos
                    .Include(p => p.Estoque)
                    .FirstOrDefaultAsync(p => p.IdProd == id);

                if (produto == null)
                {
                    response.status = false;
                    response.Mensagem = "Produto não encontrado.";
                    response.Dados = false;
                    return response;
                }

                var imagemPath = produto.ImgUrl;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Remover estoques associados
                    if (produto.Estoque != null && produto.Estoque.Any())
                    {
                        _context.Estoques.RemoveRange(produto.Estoque);
                        _logger.LogInformation("Removidos {Count} estoques do produto {ProdutoId}", produto.Estoque.Count, id);
                    }

                    // Remover produto
                    _context.Produtos.Remove(produto);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Deletar imagem se existir
                    if (!string.IsNullOrWhiteSpace(imagemPath))
                    {
                        try
                        {
                            _imageService.DeleteImage(imagemPath);
                            _logger.LogInformation("Imagem {ImagePath} deletada do produto {ProdutoId}", imagemPath, id);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "Erro ao deletar imagem {ImagePath} do produto {ProdutoId}", imagemPath, id);
                        }
                    }

                    response.status = true;
                    response.Mensagem = "Produto excluído com sucesso.";
                    response.Dados = true;

                    _logger.LogInformation("Produto {ProdutoId} excluído com sucesso", id);

                    return response;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir produto {ProdutoId}: {Message}", id, ex.Message);
                response.status = false;
                response.Mensagem = $"Erro interno ao excluir produto: {ex.Message}";
                response.Dados = false;
                return response;
            }
        }

        #region Métodos Auxiliares Privados

        private static ProdutoResponseDto CreateProdutoResponseDto(Produto produto)
        {
            return new ProdutoResponseDto
            {
                IdProd = produto.IdProd,
                Preco = produto.Preco,
                IdCategoria = produto.IdCategoria,
                IdModelo = produto.IdModelo,
                IdTecido = produto.IdTecido ?? 0,
                IdStatus = produto.IdStatus,
                Descricao = produto.descricao,
                ImgUrl = produto.ImgUrl,
                CategoriaNome = produto.IdCategoriaNavigation?.Categoria1 ?? "Categoria não encontrada",
                ModeloNome = produto.IdModeloNavigation?.Modelo1 ?? "Modelo não encontrado",
                TecidoNome = produto.IdTecidoNavigation?.Tipo ?? "Tecido não encontrado",
                StatusNome = produto.IdStatusNavigation?.Descricao ?? "Status não encontrado",
                TamanhosQuantidades = produto.Estoque?.Select(e => new TamanhoQuantidadeDto
                {
                    Tamanho = e.Tamanho,
                    Quantidade = e.Quantidade
                }).OrderBy(tq => tq.Tamanho).ToList() ?? new List<TamanhoQuantidadeDto>()
            };
        }

        private async Task<bool> ValidarReferenciasAsync(int categoriaId, int modeloId, int tecidoId)
        {
            try
            {
                var categoriaExiste = await _context.Categorias
                    .AsNoTracking()
                    .AnyAsync(c => c.IdCategoria == categoriaId);

                var modeloExiste = await _context.Modelos
                    .AsNoTracking()
                    .AnyAsync(m => m.IdModelo == modeloId);

                var tecidoExiste = await _context.Tecidos
                    .AsNoTracking()
                    .AnyAsync(t => t.IdTecido == tecidoId);

                return categoriaExiste && modeloExiste && tecidoExiste;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar referências: Categoria={CategoriaId}, Modelo={ModeloId}, Tecido={TecidoId}",
                    categoriaId, modeloId, tecidoId);
                return false;
            }
        }

        #endregion
    }
}

// Services/ProdutoService.cs - ATUALIZADO PARA MÚLTIPLAS IMAGENS
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
                    .Include(p => p.ProdutoImagens.OrderBy(pi => pi.OrdemExibicao))
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
                    .Include(p => p.ProdutoImagens.OrderBy(pi => pi.OrdemExibicao))
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
                    .Include(p => p.ProdutoImagens.OrderBy(pi => pi.OrdemExibicao))
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

            // Validações iniciais
            if (produtoDto.Imagens == null || !produtoDto.Imagens.Any())
            {
                response.status = false;
                response.Mensagem = "Pelo menos uma imagem é obrigatória.";
                return response;
            }

            if (produtoDto.Imagens.Count > 4)
            {
                response.status = false;
                response.Mensagem = "Máximo de 4 imagens permitidas.";
                return response;
            }

            // Validar cada imagem
            foreach (var imagem in produtoDto.Imagens)
            {
                if (!_imageService.IsValidImageFile(imagem))
                {
                    response.status = false;
                    response.Mensagem = $"A imagem '{imagem.FileName}' não é um formato válido.";
                    return response;
                }
            }

            if (!await ValidarReferenciasAsync(produtoDto.IdCategoria, produtoDto.IdModelo, produtoDto.IdTecido))
            {
                response.status = false;
                response.Mensagem = "Uma ou mais referências (categoria, modelo, tecido) não foram encontradas.";
                return response;
            }

            var imagensSalvas = new List<string>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Salvar todas as imagens primeiro
                for (int i = 0; i < produtoDto.Imagens.Count; i++)
                {
                    var imagePath = await _imageService.SaveImageAsync(produtoDto.Imagens[i]);
                    imagensSalvas.Add(imagePath);
                    _logger.LogInformation("Imagem {Index} salva: {Path}", i + 1, imagePath);
                }



                // 2) Criar e inserir produto
                var novoProduto = new Produto
                {
                    Preco = produtoDto.Preco,
                    IdCategoria = produtoDto.IdCategoria,
                    IdModelo = produtoDto.IdModelo,
                    IdTecido = produtoDto.IdTecido,
                    IdStatus = STATUS_DISPONIVEL,
                    ImgUrl = imagensSalvas.First(), // Manter compatibilidade - primeira imagem
                    descricao = produtoDto.Descricao
                };

                _context.Produtos.Add(novoProduto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Produto criado com ID: {ProdutoId}", novoProduto.IdProd);
                if (novoProduto.IdProd <= 0)
                {
                    throw new InvalidOperationException("ID do produto não foi gerado corretamente.");
                }

                // 3) Inserir imagens na tabela produto_imagem
                var produtoImagens = new List<ProdutoImagem>();
                for (int i = 0; i < imagensSalvas.Count; i++)
                {
                    var produtoImagem = new ProdutoImagem
                    {
                        IdProduto = novoProduto.IdProd,
                        ImgUrl = imagensSalvas[i],
                        OrdemExibicao = (byte)(i + 1),
                        IsPrincipal = i == 0, // Primeira imagem é principal
                        DataCriacao = DateTime.Now
                    };
                    produtoImagens.Add(produtoImagem);
                }

                await _context.ProdutoImagens.AddRangeAsync(produtoImagens);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inseridas {Count} imagens para o produto {ProdutoId}", produtoImagens.Count, novoProduto.IdProd);

                // 4) Inserir estoques
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

                        await _context.Estoques.AddRangeAsync(estoques);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Estoques inseridos com sucesso para ProdutoID={ProdutoId}", novoProduto.IdProd);
                    }
                }

                await transaction.CommitAsync();

                // 5) Recuperar produto completo para retornar
                var produtoCriado = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .Include(p => p.ProdutoImagens.OrderBy(pi => pi.OrdemExibicao))
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

                // Limpar imagens salvas em caso de erro
                foreach (var imagemPath in imagensSalvas)
                {
                    try { _imageService.DeleteImage(imagemPath); }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Erro ao deletar imagem durante rollback: {Path}", imagemPath);
                    }
                }

                var detalhe = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "❌ ERRO ao criar produto com múltiplas imagens. {Detalle}", detalhe);
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
                .Include(p => p.ProdutoImagens)
                .FirstOrDefaultAsync(p => p.IdProd == id);

            if (produtoExistente == null)
            {
                response.status = false;
                response.Mensagem = "Produto não encontrado.";
                return response;
            }

            // Validar referências
            if (!await ValidarReferenciasAsync(produtoDto.IdCategoria, produtoDto.IdModelo, produtoDto.IdTecido))
            {
                response.status = false;
                response.Mensagem = "Uma ou mais referências não foram encontradas.";
                return response;
            }

            // Validar status
            if (produtoDto.IdStatus != STATUS_DISPONIVEL && produtoDto.IdStatus != STATUS_INDISPONIVEL)
            {
                response.status = false;
                response.Mensagem = "Status deve ser 1 (Disponível) ou 2 (Indisponível).";
                return response;
            }

            // Validar novas imagens se fornecidas
            if (produtoDto.NovasImagens != null)
            {
                foreach (var imagem in produtoDto.NovasImagens)
                {
                    if (!_imageService.IsValidImageFile(imagem))
                    {
                        response.status = false;
                        response.Mensagem = $"A imagem '{imagem.FileName}' não é um formato válido.";
                        return response;
                    }
                }
            }

            var novasImagensSalvas = new List<string>();
            var imagensParaRemover = new List<string>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Salvar novas imagens se fornecidas
                if (produtoDto.NovasImagens != null && produtoDto.NovasImagens.Any())
                {
                    foreach (var novaImagem in produtoDto.NovasImagens)
                    {
                        var imagePath = await _imageService.SaveImageAsync(novaImagem);
                        novasImagensSalvas.Add(imagePath);
                        _logger.LogInformation("Nova imagem salva para atualização: {Path}", imagePath);
                    }
                }

                // 2) Gerenciar imagens existentes
                var imagensExistentes = produtoExistente.ProdutoImagens.ToList();
                var imagensParaManter = produtoDto.ImagensParaManter ?? new List<int>();

                // Identificar imagens para remover
                var imagensParaExcluir = imagensExistentes
                    .Where(img => !imagensParaManter.Contains(img.IdProdutoImagem))
                    .ToList();

                foreach (var imagemParaExcluir in imagensParaExcluir)
                {
                    imagensParaRemover.Add(imagemParaExcluir.ImgUrl);
                    _context.ProdutoImagens.Remove(imagemParaExcluir);
                }

                // 3) Adicionar novas imagens à tabela produto_imagem
                if (novasImagensSalvas.Any())
                {
                    var proximaOrdem = (byte)(imagensExistentes.Count(img => imagensParaManter.Contains(img.IdProdutoImagem)) + 1);

                    foreach (var novaImagemPath in novasImagensSalvas)
                    {
                        var novaProdutoImagem = new ProdutoImagem
                        {
                            IdProduto = id,
                            ImgUrl = novaImagemPath,
                            OrdemExibicao = proximaOrdem++,
                            IsPrincipal = false, // Por padrão, novas imagens não são principais
                            DataCriacao = DateTime.Now
                        };
                        _context.ProdutoImagens.Add(novaProdutoImagem);
                    }
                }

                // 4) Atualizar dados do produto
                produtoExistente.Preco = produtoDto.Preco;
                produtoExistente.IdCategoria = produtoDto.IdCategoria;
                produtoExistente.IdModelo = produtoDto.IdModelo;
                produtoExistente.IdTecido = produtoDto.IdTecido;
                produtoExistente.IdStatus = produtoDto.IdStatus;
                produtoExistente.descricao = produtoDto.Descricao;

                // Atualizar ImgUrl para manter compatibilidade (primeira imagem restante)
                var primeiraImagemRestante = await _context.ProdutoImagens
                    .Where(pi => pi.IdProduto == id)
                    .OrderBy(pi => pi.OrdemExibicao)
                    .FirstOrDefaultAsync();

                if (primeiraImagemRestante != null)
                {
                    produtoExistente.ImgUrl = primeiraImagemRestante.ImgUrl;
                }
                else if (novasImagensSalvas.Any())
                {
                    produtoExistente.ImgUrl = novasImagensSalvas.First();
                }

                // 5) Atualizar estoques
                await AtualizarEstoquesAsync(id, produtoDto.TamanhosQuantidades);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 6) Deletar imagens físicas removidas
                foreach (var imagemPath in imagensParaRemover)
                {
                    try
                    {
                        _imageService.DeleteImage(imagemPath);
                        _logger.LogInformation("Imagem removida: {Path}", imagemPath);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Erro ao deletar imagem física: {Path}", imagemPath);
                    }
                }

                // 7) Buscar produto atualizado
                var produtoAtualizado = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Include(p => p.Estoque)
                    .Include(p => p.ProdutoImagens.OrderBy(pi => pi.OrdemExibicao))
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

                // Limpar novas imagens salvas em caso de erro
                foreach (var imagemPath in novasImagensSalvas)
                {
                    try { _imageService.DeleteImage(imagemPath); }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Erro ao deletar nova imagem durante rollback: {Path}", imagemPath);
                    }
                }

                var inner = ex.InnerException?.Message;
                _logger.LogError(ex, "Erro ao atualizar produto {ProdutoId}: {Message}. InnerException: {Inner}", id, ex.Message, inner);
                response.status = false;
                response.Mensagem = $"Erro interno ao atualizar produto: {ex.Message}";
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
                    .Include(p => p.ProdutoImagens)
                    .FirstOrDefaultAsync(p => p.IdProd == id);

                if (produto == null)
                {
                    response.status = false;
                    response.Mensagem = "Produto não encontrado.";
                    response.Dados = false;
                    return response;
                }

                var imagensPaths = produto.ProdutoImagens.Select(pi => pi.ImgUrl).ToList();

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Remover imagens da tabela produto_imagem
                    if (produto.ProdutoImagens.Any())
                    {
                        _context.ProdutoImagens.RemoveRange(produto.ProdutoImagens);
                        _logger.LogInformation("Removidas {Count} imagens do produto {ProdutoId}", produto.ProdutoImagens.Count, id);
                    }

                    // Remover estoques associados
                    if (produto.Estoque.Any())
                    {
                        _context.Estoques.RemoveRange(produto.Estoque);
                        _logger.LogInformation("Removidos {Count} estoques do produto {ProdutoId}", produto.Estoque.Count, id);
                    }

                    // Remover produto
                    _context.Produtos.Remove(produto);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Deletar arquivos físicos das imagens
                    foreach (var imagePath in imagensPaths)
                    {
                        try
                        {
                            _imageService.DeleteImage(imagePath);
                            _logger.LogInformation("Imagem {ImagePath} deletada do produto {ProdutoId}", imagePath, id);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "Erro ao deletar imagem {ImagePath} do produto {ProdutoId}", imagePath, id);
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
                ImgUrl = produto.ImgUrl, // Mantido para compatibilidade
                CategoriaNome = produto.IdCategoriaNavigation?.Categoria1 ?? "Categoria não encontrada",
                ModeloNome = produto.IdModeloNavigation?.Modelo1 ?? "Modelo não encontrado",
                TecidoNome = produto.IdTecidoNavigation?.Tipo ?? "Tecido não encontrado",
                StatusNome = produto.IdStatusNavigation?.Descricao ?? "Status não encontrado",

                // NOVA PROPRIEDADE - Lista de todas as imagens
                Imagens = produto.ProdutoImagens?.Select(pi => new ProdutoImagemDto
                {
                    IdProdutoImagem = pi.IdProdutoImagem,
                    ImgUrl = pi.ImgUrl,
                    OrdemExibicao = pi.OrdemExibicao,
                    IsPrincipal = pi.IsPrincipal
                }).OrderBy(img => img.OrdemExibicao).ToList() ?? new List<ProdutoImagemDto>(),

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

        private async Task AtualizarEstoquesAsync(int produtoId, List<TamanhoQuantidadeDto> tamanhosQuantidades)
        {
            // Remover estoques existentes
            var estoquesExistentes = await _context.Estoques
                .Where(e => e.IdProduto == produtoId)
                .ToListAsync();

            if (estoquesExistentes.Any())
            {
                _context.Estoques.RemoveRange(estoquesExistentes);
                _logger.LogInformation("Removidos {Count} estoques existentes do produto {ProdutoId}", estoquesExistentes.Count, produtoId);
            }

            // Adicionar novos estoques
            if (tamanhosQuantidades != null && tamanhosQuantidades.Any())
            {
                var tamanhosValidos = tamanhosQuantidades
                    .Where(tq => !string.IsNullOrWhiteSpace(tq.Tamanho))
                    .ToList();

                if (tamanhosValidos.Any())
                {
                    var novosEstoques = tamanhosValidos
                        .Select(tq => new Estoque
                        {
                            IdProduto = produtoId,
                            Tamanho = tq.Tamanho.Trim().ToUpper(),
                            Quantidade = Math.Max(0, tq.Quantidade)
                        })
                        .GroupBy(e => e.Tamanho)
                        .Select(g => new Estoque
                        {
                            IdProduto = produtoId,
                            Tamanho = g.Key,
                            Quantidade = g.Sum(e => e.Quantidade)
                        })
                        .ToList();

                    await _context.Estoques.AddRangeAsync(novosEstoques);
                    _logger.LogInformation("Novos estoques adicionados: {Count} itens para Produto {ProdutoId}", novosEstoques.Count, produtoId);
                }
            }
        }

        #endregion
    }
}
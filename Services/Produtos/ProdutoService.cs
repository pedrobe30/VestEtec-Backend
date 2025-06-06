using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _context;
        private readonly IImageService _imageService;
        private readonly ILogger<ProdutoService> _logger;

        // Constante para o status padrão - torna o código mais legível e manutenível
        private const int STATUS_DISPONIVEL = 1;

        public ProdutoService(AppDbContext context, IImageService imageService, ILogger<ProdutoService> logger)
        {
            _context = context;
            _imageService = imageService;
            _logger = logger;
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
                        CategoriaNome = p.IdCategoriaNavigation.Categoria1,
                        ModeloNome = p.IdModeloNavigation.Modelo1,
                        TecidoNome = p.IdTecido.HasValue ? p.IdTecidoNavigation.Tipo : null,
                        StatusNome = p.IdStatusNavigation.Descricao
                    })
                    .ToListAsync();

                response.Dados = produtos;
                response.Mensagem = "Produtos recuperados com sucesso";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar produtos");
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

                response.Dados = CreateProdutoDto(produto);
                response.Mensagem = "Produto recuperado com sucesso";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar produto com ID {Id}", id);
                response.status = false;
                response.Mensagem = $"Erro ao recuperar produto: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<List<ProdutoDto>>> GetProdutosByCategoriaAsync(int categoriaId)
        {
            var response = new ResponseModel<List<ProdutoDto>>();

            try
            {
                // Verificar se a categoria existe
                var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == categoriaId);

                if (!categoriaExists)
                {
                    response.status = false;
                    response.Mensagem = "Categoria não encontrada";
                    return response;
                }

                // Buscar produtos da categoria
                var produtos = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .Where(p => p.IdCategoria == categoriaId)
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
                        CategoriaNome = p.IdCategoriaNavigation.Categoria1,
                        ModeloNome = p.IdModeloNavigation.Modelo1,
                        TecidoNome = p.IdTecido.HasValue ? p.IdTecidoNavigation.Tipo : null,
                        StatusNome = p.IdStatusNavigation.Descricao
                    })
                    .ToListAsync();

                response.Dados = produtos;
                response.Mensagem = produtos.Any()
                    ? $"Produtos da categoria {categoriaId} recuperados com sucesso"
                    : $"Nenhum produto encontrado para a categoria {categoriaId}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar produtos da categoria {CategoriaId}", categoriaId);
                response.status = false;
                response.Mensagem = $"Erro ao recuperar produtos por categoria: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<ProdutoDto>> AddProdutoCompletoAsync(ProdutoDto produtoDto)
        {
            var response = new ResponseModel<ProdutoDto>();

            try
            {
                _logger.LogInformation("Iniciando adição de produto");

                // Validar a imagem primeiro
                if (produtoDto.Imagem == null)
                {
                    response.status = false;
                    response.Mensagem = "Imagem é obrigatória";
                    return response;
                }

                if (!_imageService.IsValidImageFile(produtoDto.Imagem))
                {
                    response.status = false;
                    response.Mensagem = "Arquivo de imagem inválido. Permitidos: JPG, JPEG, PNG, GIF, BMP, WEBP (máximo 5MB)";
                    return response;
                }

                // Validar se as referências existem no banco
                var validationResult = await ValidateReferencesAsync(
                    produtoDto.IdCategoria,
                    produtoDto.IdModelo,
                    produtoDto.IdTecido
                );

                if (!validationResult.IsValid)
                {
                    response.status = false;
                    response.Mensagem = validationResult.ErrorMessage;
                    return response;
                }

                // Salvar a imagem
                string imagePath;
                try
                {
                    _logger.LogInformation("Salvando imagem do produto");
                    imagePath = await _imageService.SaveImageAsync(produtoDto.Imagem);
                    _logger.LogInformation($"Imagem salva em: {imagePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao salvar imagem");
                    response.status = false;
                    response.Mensagem = $"Erro ao salvar imagem: {ex.Message}";
                    return response;
                }

                // Criar o novo produto
                var novoProduto = new Produto
                {
                    Preco = produtoDto.Preco,
                    QuantEstoque = produtoDto.QuantEstoque,
                    IdCategoria = produtoDto.IdCategoria,
                    IdModelo = produtoDto.IdModelo,
                    IdTecido = produtoDto.IdTecido,
                    IdStatus = STATUS_DISPONIVEL,
                    ImgUrl = imagePath
                };

                _context.Produtos.Add(novoProduto);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Produto criado com ID: {novoProduto.IdProd}");

                // Diagnóstico para entender o problema
                var diagnostico = await DiagnosticarRelacionamentos(novoProduto.IdProd);
                _logger.LogInformation($"Diagnóstico: {diagnostico}");

                // Tentar recarregar o produto com retry e fallback
                ProdutoDto produtoRetorno = null;

                try
                {
                    // Primeira tentativa: recarregar com todas as propriedades de navegação
                    var produtoCompleto = await _context.Produtos
                        .Include(p => p.IdCategoriaNavigation)
                        .Include(p => p.IdModeloNavigation)
                        .Include(p => p.IdTecidoNavigation)
                        .Include(p => p.IdStatusNavigation)
                        .FirstOrDefaultAsync(p => p.IdProd == novoProduto.IdProd);

                    if (produtoCompleto != null)
                    {
                        produtoRetorno = CreateProdutoDto(produtoCompleto);
                        _logger.LogInformation("Produto recarregado com sucesso usando Include");
                    }
                    else
                    {
                        _logger.LogWarning("Produto não encontrado com Include, tentando abordagem manual");

                        // Fallback: buscar o produto básico e as referências separadamente
                        var produtoBasico = await _context.Produtos.FindAsync(novoProduto.IdProd);

                        if (produtoBasico == null)
                        {
                            _logger.LogError($"Produto com ID {novoProduto.IdProd} não encontrado no banco após criação");
                            response.status = false;
                            response.Mensagem = "Erro crítico: produto não encontrado após criação";
                            return response;
                        }

                        // Buscar as referências manualmente
                        var categoria = await _context.Categorias.FindAsync(produtoBasico.IdCategoria);
                        var modelo = await _context.Modelos.FindAsync(produtoBasico.IdModelo);
                        var tecido = produtoBasico.IdTecido.HasValue ?
                            await _context.Tecidos.FindAsync(produtoBasico.IdTecido.Value) : null;
                        var status = await _context.Statuses.FindAsync(produtoBasico.IdStatus);

                        // Criar o DTO manualmente
                        produtoRetorno = new ProdutoDto
                        {
                            IdProd = produtoBasico.IdProd,
                            Preco = produtoBasico.Preco,
                            QuantEstoque = produtoBasico.QuantEstoque,
                            IdCategoria = produtoBasico.IdCategoria,
                            IdModelo = produtoBasico.IdModelo,
                            IdTecido = produtoBasico.IdTecido,
                            IdStatus = produtoBasico.IdStatus,
                            ImgUrl = produtoBasico.ImgUrl,
                            CategoriaNome = categoria?.Categoria1,
                            ModeloNome = modelo?.Modelo1,
                            TecidoNome = tecido?.Tipo,
                            StatusNome = status?.Descricao
                        };

                        _logger.LogInformation("Produto criado usando busca manual das referências");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao recarregar produto após criação");

                    // Último fallback: retornar dados básicos do produto criado
                    produtoRetorno = new ProdutoDto
                    {
                        IdProd = novoProduto.IdProd,
                        Preco = novoProduto.Preco,
                        QuantEstoque = novoProduto.QuantEstoque,
                        IdCategoria = novoProduto.IdCategoria,
                        IdModelo = novoProduto.IdModelo,
                        IdTecido = novoProduto.IdTecido,
                        IdStatus = novoProduto.IdStatus,
                        ImgUrl = novoProduto.ImgUrl,
                        // Nomes das referências ficarão null, mas o produto foi criado
                        CategoriaNome = null,
                        ModeloNome = null,
                        TecidoNome = null,
                        StatusNome = null
                    };

                    _logger.LogWarning("Retornando produto com dados básicos devido a erro no recarregamento");
                }

                // Preparar resposta com todos os dados disponíveis
                response.Dados = produtoRetorno;
                response.Mensagem = "Produto adicionado com sucesso";

                _logger.LogInformation($"Produto retornado com dados completos: ID={response.Dados.IdProd}, Categoria={response.Dados.CategoriaNome}, Modelo={response.Dados.ModeloNome}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar produto");
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

                // Validar imagem apenas se uma nova imagem foi enviada
                if (produtoDto.Imagem != null && !_imageService.IsValidImageFile(produtoDto.Imagem))
                {
                    response.status = false;
                    response.Mensagem = "Arquivo de imagem inválido. Permitidos: JPG, JPEG, PNG, GIF, BMP, WEBP (máximo 5MB)";
                    return response;
                }

                // Validar referências
                var validationResult = await ValidateReferencesAsync(
                    produtoDto.IdCategoria,
                    produtoDto.IdModelo,
                    produtoDto.IdTecido,
                    produtoDto.IdStatus // No update, o status pode ser alterado
                );

                if (!validationResult.IsValid)
                {
                    response.status = false;
                    response.Mensagem = validationResult.ErrorMessage;
                    return response;
                }

                // Atualizar imagem se uma nova foi enviada
                if (produtoDto.Imagem != null)
                {
                    string novaImagemPath;
                    try
                    {
                        novaImagemPath = await _imageService.SaveImageAsync(produtoDto.Imagem);

                        // Deletar a imagem antiga se existir
                        if (!string.IsNullOrEmpty(produto.ImgUrl))
                        {
                            _imageService.DeleteImage(produto.ImgUrl);
                        }

                        produto.ImgUrl = novaImagemPath;
                    }
                    catch (Exception ex)
                    {
                        response.status = false;
                        response.Mensagem = $"Erro ao salvar nova imagem: {ex.Message}";
                        return response;
                    }
                }

                // Atualizar propriedades do produto
                produto.Preco = produtoDto.Preco;
                produto.QuantEstoque = produtoDto.QuantEstoque;
                produto.IdCategoria = produtoDto.IdCategoria;
                produto.IdModelo = produtoDto.IdModelo;
                produto.IdTecido = produtoDto.IdTecido;
                produto.IdStatus = produtoDto.IdStatus;

                _context.Produtos.Update(produto);
                await _context.SaveChangesAsync();

                // Recarregar com propriedades de navegação
                var produtoAtualizado = await _context.Produtos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdModeloNavigation)
                    .Include(p => p.IdTecidoNavigation)
                    .Include(p => p.IdStatusNavigation)
                    .FirstOrDefaultAsync(p => p.IdProd == id);

                if (produtoAtualizado == null)
                {
                    response.status = false;
                    response.Mensagem = "Erro ao recuperar produto atualizado";
                    return response;
                }

                response.Dados = CreateProdutoDto(produtoAtualizado);
                response.Mensagem = "Produto atualizado com sucesso";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto com ID {Id}", id);
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

                // Deletar imagem associada se existir
                if (!string.IsNullOrEmpty(produto.ImgUrl))
                {
                    _imageService.DeleteImage(produto.ImgUrl);
                }

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                response.Dados = true;
                response.Mensagem = "Produto excluído com sucesso";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir produto com ID {Id}", id);
                response.status = false;
                response.Mensagem = $"Erro ao excluir produto: {ex.Message}";
                response.Dados = false;
            }

            return response;
        }

        // MÉTODOS AUXILIARES

        /// <summary>
        /// Valida se todas as referências existem no banco de dados
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage)> ValidateReferencesAsync(
            int categoriaId, int modeloId, int? tecidoId, int? statusId = null)
        {
            var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == categoriaId);
            var modeloExists = await _context.Modelos.AnyAsync(m => m.IdModelo == modeloId);
            var tecidoExists = tecidoId.HasValue ?
                await _context.Tecidos.AnyAsync(t => t.IdTecido == tecidoId) : true;
            var statusExists = statusId.HasValue ?
                await _context.Statuses.AnyAsync(s => s.IdStatus == statusId) : true;

            if (!categoriaExists)
                return (false, "Categoria não encontrada");
            if (!modeloExists)
                return (false, "Modelo não encontrado");
            if (!tecidoExists)
                return (false, "Tecido não encontrado");
            if (!statusExists)
                return (false, "Status não encontrado");

            return (true, string.Empty);
        }

        /// <summary>
        /// Cria um ProdutoDto a partir de um Produto com navigation properties carregadas
        /// </summary>
        private ProdutoDto CreateProdutoDto(Produto produto)
        {
            return new ProdutoDto
            {
                IdProd = produto.IdProd,
                Preco = produto.Preco,
                QuantEstoque = produto.QuantEstoque,
                IdCategoria = produto.IdCategoria,
                IdModelo = produto.IdModelo,
                IdTecido = produto.IdTecido,
                IdStatus = produto.IdStatus,
                ImgUrl = produto.ImgUrl,
                CategoriaNome = produto.IdCategoriaNavigation?.Categoria1,
                ModeloNome = produto.IdModeloNavigation?.Modelo1,
                TecidoNome = produto.IdTecido.HasValue ? produto.IdTecidoNavigation?.Tipo : null,
                StatusNome = produto.IdStatusNavigation?.Descricao
            };
        }

        /// <summary>
        /// Método auxiliar para diagnosticar problemas de relacionamento
        /// </summary>
        private async Task<string> DiagnosticarRelacionamentos(int produtoId)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(produtoId);
                if (produto == null) return $"Produto {produtoId} não existe";

                var diagnostico = new List<string>
                {
                    $"Produto ID: {produto.IdProd}",
                    $"Categoria ID: {produto.IdCategoria}",
                    $"Modelo ID: {produto.IdModelo}",
                    $"Tecido ID: {produto.IdTecido}",
                    $"Status ID: {produto.IdStatus}"
                };

                // Verificar se as referências existem
                var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == produto.IdCategoria);
                var modeloExists = await _context.Modelos.AnyAsync(m => m.IdModelo == produto.IdModelo);
                var tecidoExists = produto.IdTecido.HasValue ?
                    await _context.Tecidos.AnyAsync(t => t.IdTecido == produto.IdTecido) : true;
                var statusExists = await _context.Statuses.AnyAsync(s => s.IdStatus == produto.IdStatus);

                diagnostico.Add($"Categoria existe: {categoriaExists}");
                diagnostico.Add($"Modelo existe: {modeloExists}");
                diagnostico.Add($"Tecido existe: {tecidoExists}");
                diagnostico.Add($"Status existe: {statusExists}");

                return string.Join(" | ", diagnostico);
            }
            catch (Exception ex)
            {
                return $"Erro no diagnóstico: {ex.Message}";
            }
        }
    }
}
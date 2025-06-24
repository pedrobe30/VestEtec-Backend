// Services/EncomendaService.cs - CORRIGIDO

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
    public class EncomendaService : IEncomendaService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EncomendaService> _logger;

        public EncomendaService(AppDbContext context, ILogger<EncomendaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- MÉTODOS PARA ALUNOS ---

        public async Task<EncomendaDto> CriarEncomendaAsync(CriarEncomendaDto criarEncomendaDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validar aluno
                var aluno = await _context.Alunos.FindAsync(criarEncomendaDto.IdAluno);
                if (aluno == null)
                    throw new Exception("Aluno não encontrado");

                if (criarEncomendaDto.IdEscola != aluno.IdEsc)
                    throw new Exception("Escola inválida para o aluno informado");

                decimal valorTotal = 0;
                var itensEntidade = new List<Itensencomendado>();

                // 2. Processar cada item do carrinho
                foreach (var itemCarrinho in criarEncomendaDto.Itens)
                {
                    // 2.1 Validar produto
                    var produto = await _context.Produtos.FindAsync(itemCarrinho.IdProduto);
                    if (produto == null)
                        throw new Exception($"Produto com ID {itemCarrinho.IdProduto} não encontrado.");

                    // 2.2 Validar e atualizar estoque
                    var estoqueDoItem = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.IdProduto == itemCarrinho.IdProduto &&
                                                 e.Tamanho == itemCarrinho.Tamanho.Trim().ToUpper());

                    if (estoqueDoItem == null)
                        throw new Exception($"O produto '{produto.IdProd}' não está disponível no tamanho '{itemCarrinho.Tamanho}'.");

                    if (estoqueDoItem.Quantidade < itemCarrinho.Quantidade)
                        throw new Exception($"Estoque insuficiente para o produto '{produto.IdProd}' no tamanho '{itemCarrinho.Tamanho}'. Disponível: {estoqueDoItem.Quantidade}, Solicitado: {itemCarrinho.Quantidade}");

                    // 2.3 Descontar do estoque
                    estoqueDoItem.Quantidade -= itemCarrinho.Quantidade;
                    _context.Estoques.Update(estoqueDoItem);

                    // 2.4 Calcular valor total
                    valorTotal += produto.Preco * itemCarrinho.Quantidade;

                    // 2.5 Preparar item da encomenda (SEM IdEncomenda ainda)
                    itensEntidade.Add(new Itensencomendado
                    {
                        IdProduto = itemCarrinho.IdProduto,
                        Quantidade = itemCarrinho.Quantidade,
                        Tamanho = itemCarrinho.Tamanho.Trim().ToUpper()
                        // IdEncomenda será definido após salvar a encomenda
                    });
                }

                // 3. Criar encomenda
                var encomenda = new Encomenda
                {
                    IdAluno = criarEncomendaDto.IdAluno,
                    IdEscola = criarEncomendaDto.IdEscola,
                    DataEncomenda = DateTime.Now,
                    PrecoEncomenda = valorTotal,
                    Situacao = "PENDENTE",
                    DataEntrega = DateTime.Now.AddDays(7)
                };

                _context.Encomendas.Add(encomenda);

                // 4. Salvar encomenda primeiro para obter o ID
                await _context.SaveChangesAsync();

                _logger.LogInformation("Encomenda criada com ID: {EncomendaId}", encomenda.IdEncomenda);

                // 5. Agora definir o IdEncomenda nos itens e salvá-los
                foreach (var item in itensEntidade)
                {
                    item.IdEncomenda = encomenda.IdEncomenda;
                }

                _context.Itensencomendados.AddRange(itensEntidade);

                // 6. Salvar tudo
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Encomenda {EncomendaId} criada com sucesso com {TotalItens} itens",
                    encomenda.IdEncomenda, itensEntidade.Count);

                // 7. Retornar encomenda completa
                return await ObterEncomendaCompletaAsync(encomenda.IdEncomenda);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro na transação de criar encomenda: {Message}", ex.Message);

                // Log da inner exception se existir
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {InnerMessage}", ex.InnerException.Message);
                }

                throw;
            }
        }

        public async Task<List<EncomendaAlunoDto>> ObterEncomendaspPorAlunoAsync(int idAluno)
        {
            var encomendas = await _context.Encomendas
                .Where(e => e.IdAluno == idAluno)
                .OrderByDescending(e => e.DataEncomenda)
                .Select(e => new EncomendaAlunoDto
                {
                    IdEncomenda = e.IdEncomenda,
                    DataEncomenda = e.DataEncomenda,
                    PrecoEncomenda = e.PrecoEncomenda,
                    Situacao = NormalizarStatus(e.Situacao),
                    DataEntrega = e.DataEntrega,
                    TotalItens = _context.Itensencomendados.Count(i => i.IdEncomenda == e.IdEncomenda)
                })
                .ToListAsync();
            return encomendas;
        }

        public async Task<EncomendaAlunoDto> ObterEncomendaDetalhadaAsync(int idEncomenda, int idAluno)
        {
            var encomenda = await _context.Encomendas
                .Include(e => e.Itensencomendados)
                    .ThenInclude(i => i.IdProdutoNavigation)
                        .ThenInclude(p => p.IdCategoriaNavigation)
                .Include(e => e.Itensencomendados)
                    .ThenInclude(i => i.IdProdutoNavigation)
                        .ThenInclude(p => p.IdModeloNavigation)
                .FirstOrDefaultAsync(e => e.IdEncomenda == idEncomenda && e.IdAluno == idAluno);

            if (encomenda == null) return null;

            var dto = new EncomendaAlunoDto
            {
                IdEncomenda = encomenda.IdEncomenda,
                DataEncomenda = encomenda.DataEncomenda,
                PrecoEncomenda = encomenda.PrecoEncomenda,
                Situacao = NormalizarStatus(encomenda.Situacao),
                DataEntrega = encomenda.DataEntrega,
                TotalItens = encomenda.Itensencomendados.Count,
                Itens = encomenda.Itensencomendados.Select(item => new ItemEncomendaDto
                {
                    IdItem = item.IdItem,
                    IdProduto = item.IdProduto,
                    NomeProduto = $"{item.IdProdutoNavigation.IdCategoriaNavigation.Categoria1} {item.IdProdutoNavigation.IdModeloNavigation.Modelo1}",
                    ImagemUrl = item.IdProdutoNavigation.ImgUrl,
                    PrecoUnitario = item.IdProdutoNavigation.Preco,
                    Quantidade = item.Quantidade,
                    Tamanho = item.Tamanho,
                    PrecoTotal = item.IdProdutoNavigation.Preco * item.Quantidade
                }).ToList()
            };

            return dto;
        }

        public async Task<bool> CancelarEncomendaAsync(int idEncomenda, int idAluno)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var encomenda = await _context.Encomendas
                    .Include(e => e.Itensencomendados)
                    .FirstOrDefaultAsync(e => e.IdEncomenda == idEncomenda && e.IdAluno == idAluno);

                if (encomenda == null)
                {
                    _logger.LogWarning($"Tentativa de cancelar encomenda não encontrada. ID: {idEncomenda}, Aluno: {idAluno}");
                    return false;
                }

                if (NormalizarStatus(encomenda.Situacao) != "PENDENTE")
                {
                    _logger.LogWarning($"Tentativa de cancelar encomenda com status não permitido '{encomenda.Situacao}'. ID: {idEncomenda}");
                    return false;
                }

                // Retornar itens ao estoque
                foreach (var item in encomenda.Itensencomendados)
                {
                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.IdProduto == item.IdProduto && e.Tamanho == item.Tamanho);

                    if (estoque != null)
                    {
                        estoque.Quantidade += item.Quantidade;
                        _context.Estoques.Update(estoque);
                    }
                }

                encomenda.Situacao = "CANCELADA";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Encomenda {EncomendaId} cancelada e estoque restaurado", idEncomenda);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao cancelar encomenda {EncomendaId}", idEncomenda);
                return false;
            }
        }

        // --- MÉTODOS PARA ADMIN ---

        public async Task<List<EncomendaResumoDto>> ObterTodasEncomendaspPaginadasAsync(FiltroEncomendaDto filtro)
        {
            var query = _context.Encomendas
                .Include(e => e.IdAlunoNavigation)
                .Include(e => e.IdEscolaNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro.Status))
                query = query.Where(e => e.Situacao == filtro.Status);
            if (filtro.IdEscola.HasValue)
                query = query.Where(e => e.IdEscola == filtro.IdEscola.Value);
            if (filtro.DataInicio.HasValue)
                query = query.Where(e => e.DataEncomenda >= filtro.DataInicio.Value);
            if (filtro.DataFim.HasValue)
                query = query.Where(e => e.DataEncomenda <= filtro.DataFim.Value);

            return await query
                .OrderByDescending(e => e.DataEncomenda)
                .Skip((filtro.Pagina - 1) * filtro.ItensPorPagina)
                .Take(filtro.ItensPorPagina)
                .Select(e => new EncomendaResumoDto
                {
                    IdEncomenda = e.IdEncomenda,
                    DataEncomenda = e.DataEncomenda,
                    PrecoEncomenda = e.PrecoEncomenda,
                    Situacao = e.Situacao,
                    DataEntrega = e.DataEntrega,
                    NomeAluno = e.IdAlunoNavigation.NomeAlu,
                    NomeEscola = e.IdEscolaNavigation.NomeEsc,
                    TotalItens = _context.Itensencomendados.Count(i => i.IdEncomenda == e.IdEncomenda)
                }).ToListAsync();
        }

        public async Task<EncomendaDto> ObterEncomendaCompletaAsync(int idEncomenda)
        {
            var encomenda = await _context.Encomendas
                .Include(e => e.IdAlunoNavigation)
                .Include(e => e.IdEscolaNavigation)
                .Include(e => e.Itensencomendados)
                    .ThenInclude(i => i.IdProdutoNavigation)
                        .ThenInclude(p => p.IdCategoriaNavigation)
                .Include(e => e.Itensencomendados)
                    .ThenInclude(i => i.IdProdutoNavigation)
                        .ThenInclude(p => p.IdModeloNavigation)
                .FirstOrDefaultAsync(e => e.IdEncomenda == idEncomenda);

            if (encomenda == null) return null;

            return new EncomendaDto
            {
                IdEncomenda = encomenda.IdEncomenda,
                IdAluno = encomenda.IdAluno,
                NomeAluno = encomenda.IdAlunoNavigation.NomeAlu,
                EmailAluno = encomenda.IdAlunoNavigation.EmailAlu,
                IdEscola = encomenda.IdEscola,
                NomeEscola = encomenda.IdEscolaNavigation.NomeEsc,
                DataEncomenda = encomenda.DataEncomenda,
                PrecoEncomenda = encomenda.PrecoEncomenda,
                Situacao = NormalizarStatus(encomenda.Situacao),
                DataEntrega = encomenda.DataEntrega,
                Itens = encomenda.Itensencomendados.Select(item => new ItemEncomendaDto
                {
                    IdItem = item.IdItem,
                    IdProduto = item.IdProduto,
                    NomeProduto = $"{item.IdProdutoNavigation.IdCategoriaNavigation.Categoria1} {item.IdProdutoNavigation.IdModeloNavigation.Modelo1}",
                    CategoriaNome = item.IdProdutoNavigation.IdCategoriaNavigation.Categoria1,
                    ModeloNome = item.IdProdutoNavigation.IdModeloNavigation.Modelo1,
                    TecidoNome = item.IdProdutoNavigation.IdTecidoNavigation?.Tipo ?? "N/A",
                    ImagemUrl = item.IdProdutoNavigation.ImgUrl,
                    PrecoUnitario = item.IdProdutoNavigation.Preco,
                    Quantidade = item.Quantidade,
                    Tamanho = item.Tamanho,
                    PrecoTotal = item.IdProdutoNavigation.Preco * item.Quantidade
                }).ToList()
            };
        }

        public async Task<EncomendaDto> AtualizarStatusEncomendaAsync(AtualizarStatusEncomendaDto dto)
        {
            var encomenda = await _context.Encomendas.FindAsync(dto.IdEncomenda);
            if (encomenda == null) throw new Exception("Encomenda não encontrada.");

            encomenda.Situacao = dto.NovoStatus;
            if (dto.DataEntrega.HasValue) encomenda.DataEntrega = dto.DataEntrega.Value;

            await _context.SaveChangesAsync();
            return await ObterEncomendaCompletaAsync(dto.IdEncomenda);
        }

        public async Task<EncomendaDto> AtualizarDataEntregaAsync(int idEncomenda, DateTime novaDataEntrega)
        {
            var encomenda = await _context.Encomendas.FindAsync(idEncomenda);
            if (encomenda == null) throw new Exception("Encomenda não encontrada.");

            encomenda.DataEntrega = novaDataEntrega;
            await _context.SaveChangesAsync();
            return await ObterEncomendaCompletaAsync(idEncomenda);
        }

        public async Task<bool> ExcluirEncomendaAsync(int idEncomenda)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var encomenda = await _context.Encomendas
                    .Include(e => e.Itensencomendados)
                    .FirstOrDefaultAsync(e => e.IdEncomenda == idEncomenda);

                if (encomenda == null) return false;

                // Retornar itens ao estoque antes de excluir
                foreach (var item in encomenda.Itensencomendados)
                {
                    var estoque = await _context.Estoques
                        .FirstOrDefaultAsync(e => e.IdProduto == item.IdProduto && e.Tamanho == item.Tamanho);

                    if (estoque != null)
                    {
                        estoque.Quantidade += item.Quantidade;
                        _context.Estoques.Update(estoque);
                    }
                }

                _context.Itensencomendados.RemoveRange(encomenda.Itensencomendados);
                _context.Encomendas.Remove(encomenda);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao excluir encomenda {IdEncomenda}", idEncomenda);
                return false;
            }
        }

        // --- MÉTODOS DE UTILIDADE ---

        public async Task<bool> EncomendaExisteAsync(int idEncomenda) =>
            await _context.Encomendas.AnyAsync(e => e.IdEncomenda == idEncomenda);

        public async Task<bool> EncomendaPertenceAoAlunoAsync(int idEncomenda, int idAluno) =>
            await _context.Encomendas.AnyAsync(e => e.IdEncomenda == idEncomenda && e.IdAluno == idAluno);

        public async Task<bool> PodeAlterarEncomendaAsync(int idEncomenda)
        {
            var encomenda = await _context.Encomendas.FindAsync(idEncomenda);
            return encomenda != null && NormalizarStatus(encomenda.Situacao) == "PENDENTE";
        }

        public async Task<bool> AtualizarEstoquePorStatusAsync(int idEncomenda, string novoStatus)
        {
            // Implementação pode ser feita aqui se necessário
            _logger.LogInformation("Método AtualizarEstoquePorStatusAsync chamado para encomenda {IdEncomenda} com status {NovoStatus}",
                idEncomenda, novoStatus);
            return await Task.FromResult(true);
        }

        public async Task<Dictionary<string, int>> ObterEstatisticasEncomendaspAsync()
        {
            var estatisticas = await _context.Encomendas
                .GroupBy(e => e.Situacao ?? "PENDENTE")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return estatisticas.ToDictionary(e => e.Status, e => e.Count);
        }

        private static string NormalizarStatus(string status)
        {
            return string.IsNullOrWhiteSpace(status) ? "PENDENTE" : status.Trim().ToUpper();
        }
    }
}
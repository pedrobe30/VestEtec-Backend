// Interfaces/IEncomendaService.cs
using Backend_Vestetec_App.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Interfaces
{
    public interface IEncomendaService
    {
        // Métodos para Alunos
        Task<EncomendaDto> CriarEncomendaAsync(CriarEncomendaDto criarEncomendaDto);
        Task<List<EncomendaAlunoDto>> ObterEncomendaspPorAlunoAsync(int idAluno);
        Task<EncomendaAlunoDto> ObterEncomendaDetalhadaAsync(int idEncomenda, int idAluno);
        Task<bool> CancelarEncomendaAsync(int idEncomenda, int idAluno);

        // Métodos para Admin
        Task<List<EncomendaResumoDto>> ObterTodasEncomendaspPaginadasAsync(FiltroEncomendaDto filtro);
        Task<EncomendaDto> ObterEncomendaCompletaAsync(int idEncomenda);
        Task<EncomendaDto> AtualizarStatusEncomendaAsync(AtualizarStatusEncomendaDto atualizarStatusDto);
        Task<EncomendaDto> AtualizarDataEntregaAsync(int idEncomenda, DateTime novaDataEntrega);
        Task<bool> ExcluirEncomendaAsync(int idEncomenda);

        // Métodos de utilidade
        Task<bool> EncomendaExisteAsync(int idEncomenda);
        Task<bool> EncomendaPertenceAoAlunoAsync(int idEncomenda, int idAluno);
        Task<bool> PodeAlterarEncomendaAsync(int idEncomenda);
        Task<bool> AtualizarEstoquePorStatusAsync(int idEncomenda, string novoStatus);
        Task<Dictionary<string, int>> ObterEstatisticasEncomendaspAsync();
    }
}
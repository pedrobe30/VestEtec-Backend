// Controllers/AdminEncomendaController.cs (Para Administradores)
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Descomente se usar autenticação
    public class EncomendaControllerAdm : ControllerBase
    {
        private readonly IEncomendaService _encomendaService;

        public EncomendaControllerAdm(IEncomendaService encomendaService)
        {
            _encomendaService = encomendaService;
        }

        /// <summary>
        /// Obter todas as encomendas com filtros e paginação
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EncomendaResumoDto>>> ObterTodasEncomendas([FromQuery] FiltroEncomendaDto filtro)
        {
            try
            {
                var encomendas = await _encomendaService.ObterTodasEncomendaspPaginadasAsync(filtro);
                return Ok(new { success = true, data = encomendas });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obter detalhes completos de uma encomenda
        /// </summary>
        [HttpGet("{idEncomenda}")]
        public async Task<ActionResult<EncomendaDto>> ObterEncomendaCompleta(int idEncomenda)
        {
            try
            {
                var encomenda = await _encomendaService.ObterEncomendaCompletaAsync(idEncomenda);

                if (encomenda == null)
                    return NotFound(new { success = false, message = "Encomenda não encontrada." });

                return Ok(new { success = true, data = encomenda });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar status de uma encomenda
        /// </summary>
        [HttpPut("status")]
        public async Task<ActionResult<EncomendaDto>> AtualizarStatus([FromBody] AtualizarStatusEncomendaDto atualizarStatusDto)
        {
            try
            {
                var encomenda = await _encomendaService.AtualizarStatusEncomendaAsync(atualizarStatusDto);
                return Ok(new { success = true, message = "Status atualizado com sucesso!", data = encomenda });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar data de entrega de uma encomenda
        /// </summary>
        [HttpPut("{idEncomenda}/data-entrega")]
        public async Task<ActionResult<EncomendaDto>> AtualizarDataEntrega(int idEncomenda, [FromBody] DateTime novaDataEntrega)
        {
            try
            {
                var encomenda = await _encomendaService.AtualizarDataEntregaAsync(idEncomenda, novaDataEntrega);
                return Ok(new { success = true, message = "Data de entrega atualizada com sucesso!", data = encomenda });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Excluir uma encomenda
        /// </summary>
        [HttpDelete("{idEncomenda}")]
        public async Task<ActionResult> ExcluirEncomenda(int idEncomenda)
        {
            try
            {
                var sucesso = await _encomendaService.ExcluirEncomendaAsync(idEncomenda);

                if (!sucesso)
                    return NotFound(new { success = false, message = "Encomenda não encontrada." });

                return Ok(new { success = true, message = "Encomenda excluída com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obter estatísticas das encomendas
        /// </summary>
        [HttpGet("estatisticas")]
        public async Task<ActionResult<Dictionary<string, int>>> ObterEstatisticas()
        {
            try
            {
                var estatisticas = await _encomendaService.ObterEstatisticasEncomendaspAsync();
                return Ok(new { success = true, data = estatisticas });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Verificar se uma encomenda existe
        /// </summary>
        [HttpGet("{idEncomenda}/existe")]
        public async Task<ActionResult<bool>> VerificarSeEncomendaExiste(int idEncomenda)
        {
            try
            {
                var existe = await _encomendaService.EncomendaExisteAsync(idEncomenda);
                return Ok(new { success = true, data = existe });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Verificar se uma encomenda pode ser alterada
        /// </summary>
        [HttpGet("{idEncomenda}/pode-alterar")]
        public async Task<ActionResult<bool>> VerificarSePodeAlterar(int idEncomenda)
        {
            try
            {
                var podeAlterar = await _encomendaService.PodeAlterarEncomendaAsync(idEncomenda);
                return Ok(new { success = true, data = podeAlterar });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
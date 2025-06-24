// Controllers/EncomendaController.cs (Para Alunos)
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncomendaController : ControllerBase
    {
        private readonly IEncomendaService _encomendaService;

        public EncomendaController(IEncomendaService encomendaService)
        {
            _encomendaService = encomendaService;
        }

        /// <summary>
        /// Criar uma nova encomenda a partir do carrinho
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EncomendaDto>> CriarEncomenda([FromBody] CriarEncomendaDto criarEncomendaDto)
        {
            try
            {
                if (criarEncomendaDto.Itens == null || criarEncomendaDto.Itens.Count == 0)
                    return BadRequest("Carrinho vazio. Adicione produtos antes de finalizar a encomenda.");

                var encomenda = await _encomendaService.CriarEncomendaAsync(criarEncomendaDto);
                return Ok(new { success = true, message = "Encomenda criada com sucesso!", data = encomenda });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obter todas as encomendas de um aluno
        /// </summary>
        [HttpGet("aluno/{idAluno}")]
        public async Task<ActionResult<List<EncomendaAlunoDto>>> ObterEncomendaspPorAluno(int idAluno)
        {
            try
            {
                var encomendas = await _encomendaService.ObterEncomendaspPorAlunoAsync(idAluno);
                return Ok(new { success = true, data = encomendas });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obter detalhes de uma encomenda específica do aluno
        /// </summary>
        [HttpGet("{idEncomenda}/aluno/{idAluno}")]
        public async Task<ActionResult<EncomendaAlunoDto>> ObterEncomendaDetalhada(int idEncomenda, int idAluno)
        {
            try
            {
                var encomenda = await _encomendaService.ObterEncomendaDetalhadaAsync(idEncomenda, idAluno);

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
        /// Cancelar uma encomenda (apenas se estiver pendente)
        /// </summary>
        [HttpPut("{idEncomenda}/cancelar")]
        public async Task<ActionResult> CancelarEncomenda(int idEncomenda, [FromBody] int idAluno)
        {
            try
            {
                var sucesso = await _encomendaService.CancelarEncomendaAsync(idEncomenda, idAluno);

                if (!sucesso)
                    return BadRequest(new { success = false, message = "Não foi possível cancelar a encomenda. Verifique se ela existe e está pendente." });

                return Ok(new { success = true, message = "Encomenda cancelada com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
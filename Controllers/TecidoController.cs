using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Models;
using Microsoft.AspNetCore.Mvc;
using Backend_Vestetec_App.Services;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TecidoController : ControllerBase
    {
        private readonly ITecidoService _tecidoservice;

        public TecidoController(ITecidoService tecidoService)
        {
            _tecidoservice = tecidoService;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<TecidoResponseDTO>>> GetTecidos()
        {
            try
            {
                var tecidos = await _tecidoservice.GetAllTecido();

                var tecidosResponse = tecidos.Select(c => new TecidoResponseDTO
                {
                    IdTecido = c.IdTecido,
                    tipoTecido = c.Tipo
                });
                return Ok(tecidosResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]

        public async Task<ActionResult<TecidoResponseDTO>> PostTecido(AddTecido addtecido)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newTecido = new Tecido
                {
                    Tipo = addtecido.tipoTecido
                };

                var TecidoCriado = await _tecidoservice.CreateTecido(newTecido);

                var TecidoResponse = new TecidoResponseDTO
                {
                    IdTecido = TecidoCriado.IdTecido,
                    tipoTecido = TecidoCriado.Tipo
                };

                return CreatedAtAction(
                    nameof(GetTecidos),
                    new { id = TecidoResponse.IdTecido },
                    TecidoResponse
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"erro interno: {ex.Message}");
            }
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> PutTecido(int id, TecidoUpdate tecidoupdate)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                if (!await _tecidoservice.TecidoExists(id))
                {
                    return NotFound($"Tecido com ID {id} não encontrada");
                }


                var TecidoModel = new Tecido
                {
                    IdTecido = id,
                    Tipo = tecidoupdate.tipoTecido
                };


                var TecidoAtualizado = await _tecidoservice.UpdateTecido(id, TecidoModel);

                if (TecidoAtualizado == null)
                {
                    return BadRequest("Não foi possível atualizar o tecido");
                }


                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteTecido(int id)
        {
            try
            {

                if (!await _tecidoservice.DeleteTecido(id))
                {
                    return NotFound($"Tecido com ID {id} não encontrada");
                }


                var result = await _tecidoservice.DeleteTecido(id);

                if (!result)
                {
                    return BadRequest("Não foi possível excluir o Tecido");
                }


                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

    }
}
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ModelosController : ControllerBase
    {
        private readonly IModeloService  _modeloService;

        public ModelosController(IModeloService modeloService)
        {
            _modeloService = modeloService;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<ModeloResponseDTO>>> GetModelos()
        {
            try
            {
                var modelos = await _modeloService.GetAllModelosAsync();

                var modelosResponse = modelos.Select(c => new ModeloResponseDTO
                {
                    IdModelo = c.IdModelo,
                    Modelo = c.Modelo1
                });
                return Ok(modelosResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]
        
        public async Task<ActionResult<ModeloResponseDTO>> PostModelo(AddModelos addModelos)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var newModelo = new Modelo
                {
                    Modelo1 = addModelos.Modelo
                };

                var ModeloCriado = await _modeloService.CreateModelo(newModelo);

                var ModeloResponse = new ModeloResponseDTO
                {
                    IdModelo = ModeloCriado.IdModelo,
                    Modelo = ModeloCriado.Modelo1
                };

                return CreatedAtAction(
                    nameof(GetModelos),
                    new { id = ModeloResponse.IdModelo},
                    ModeloResponse
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"erro interno: {ex.Message}");
            }
        }

         [HttpPut("{id}")]
      
        public async Task<IActionResult> PutModelo(int id, ModeloUpdate modeloUpdate)
        {
            try
            {
            
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

               
                if (!await _modeloService.ModeloExists(id))
                {
                    return NotFound($"Modelo com ID {id} não encontrada");
                }

                
                var ModeloModel = new Modelo
                {
                    IdModelo = id,
                    Modelo1 = modeloUpdate.Modelo
                };

           
                var modeloAtualizado = await _modeloService.UpdateModeloAsync(id, ModeloModel);

                if (modeloAtualizado == null)
                {
                    return BadRequest("Não foi possível atualizar o modelo");
                }

            
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

    
        [HttpDelete("{id}")]
    
        public async Task<IActionResult> DeleteModelo(int id)
        {
            try
            {
                
                if (!await _modeloService.DeleteModelo(id))
                {
                    return NotFound($"Modelo com ID {id} não encontrada");
                }

               
                var result = await _modeloService.DeleteModelo(id);

                if (!result)
                {
                    return BadRequest("Não foi possível excluir o Modelo");
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
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaResponseDTO>>> GetCategorias()
        {
            try
            {
                var categorias = await _categoriaService.GetAllCategoriasAsync();

                // Mapeia os resultados para DTOs de resposta
                var categoriasResponse = categorias.Select(c => new CategoriaResponseDTO
                {
                    IdCategoria = c.IdCategoria,
                    Categoria = c.Categoria1
                });

                return Ok(categoriasResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponseDTO>> GetCategoria(int id)
        {
            try
            {
                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);

                if (categoria == null)
                {
                    return NotFound($"Categoria com ID {id} não encontrada");
                }

                var categoriaResponse = new CategoriaResponseDTO
                {
                    IdCategoria = categoria.IdCategoria,
                    Categoria = categoria.Categoria1
                };

                return Ok(categoriaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // POST: api/Categorias
        [HttpPost]
        // [Authorize] // Exige autenticação para criar nova categoria
        public async Task<ActionResult<CategoriaResponseDTO>> PostCategoria(createCategoriaDTO createCategoriaDTO)
        {
            try
            {
                // Se o modelo não for válido, retorna erros de validação
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Mapeia DTO para o modelo
                var novaCategoriaModel = new Categoria
                {
                    Categoria1 = createCategoriaDTO.Categoria
                };

                // Salva no banco via serviço
                var categoriaCriada = await _categoriaService.CreateCategoriaAsync(novaCategoriaModel);

                // Mapeia para o DTO de resposta
                var categoriaResponse = new CategoriaResponseDTO
                {
                    IdCategoria = categoriaCriada.IdCategoria,
                    Categoria = categoriaCriada.Categoria1
                };

                // Retorna resposta 201 (Created) com o objeto criado e URI para acessá-lo
                return CreatedAtAction(
                    nameof(GetCategoria),
                    new { id = categoriaResponse.IdCategoria },
                    categoriaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        //  [Authorize] // Exige autenticação para atualizar categoria
        public async Task<IActionResult> PutCategoria(int id, UpdateCategoriaDTO updateCategoriaDTO)
        {
            try
            {
                // Verifica se o modelo é válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se a categoria existe
                if (!await _categoriaService.CategoriaExistsAsync(id))
                {
                    return NotFound($"Categoria com ID {id} não encontrada");
                }

                // Mapeia DTO para o modelo
                var categoriaModel = new Categoria
                {
                    IdCategoria = id,
                    Categoria1 = updateCategoriaDTO.Categoria
                };

                // Atualiza via serviço
                var categoriaAtualizada = await _categoriaService.UpdateCategoriaAsync(id, categoriaModel);

                if (categoriaAtualizada == null)
                {
                    return BadRequest("Não foi possível atualizar a categoria");
                }

                // Retorna status 204 (No Content)
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        //  [Authorize] // Exige autenticação para excluir categoria
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            try
            {
                // Verifica se a categoria existe
                if (!await _categoriaService.CategoriaExistsAsync(id))
                {
                    return NotFound($"Categoria com ID {id} não encontrada");
                }

                // Exclui via serviço
                var result = await _categoriaService.DeleteCategoriaAsync(id);

                if (!result)
                {
                    return BadRequest("Não foi possível excluir a categoria");
                }

                // Retorna status 204 (No Content)
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
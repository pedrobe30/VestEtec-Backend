// Controllers/ProdutoController.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Vestetec_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutoController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<ProdutoDto>>>> GetAllProdutos()
        {
            var response = await _produtoService.GetAllProdutosAsync();
            return response.status ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseModel<ProdutoDto>>> GetProdutoById(int id)
        {
            var response = await _produtoService.GetProdutoByIdAsync(id);
            return response.status ? Ok(response) : NotFound(response);
        }

        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<ResponseModel<List<ProdutoDto>>>> GetProdutosByCategoria(int categoriaId)
        {
            if (categoriaId <= 0)
            {
                return BadRequest(new ResponseModel<List<ProdutoDto>>
                {
                    status = false,
                    Mensagem = "ID da categoria deve ser maior que zero",
                    Dados = new List<ProdutoDto>()
                });
            }

            // Call the service method that already exists in your ProdutoService
            var response = await _produtoService.GetProdutosByCategoriaAsync(categoriaId);

            if (response.status)
            {
                return Ok(response);
            }

            // Return the response even if no products found (it's not necessarily an error)
            return Ok(response);
        }

        [HttpPost("AddProduto")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseModel<ProdutoDto>>> AddProdutoCompleto([FromForm] ProdutoDto produtoDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                return BadRequest(new ResponseModel<ProdutoDto>
                {
                    status = false,
                    Mensagem = $"Dados inválidos: {string.Join("; ", errors.SelectMany(e => e.Errors))}"
                });
            }

            var response = await _produtoService.AddProdutoCompletoAsync(produtoDto);

            if (response.status)
            {
                // Se o produto foi criado com sucesso, retornamos status 201 (Created)
                return CreatedAtAction(
                    nameof(GetProdutoById),
                    new { id = response.Dados.IdProd },
                    response
                );
            }

            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseModel<ProdutoDto>>> UpdateProdutoComImagem(
            int id,
            [FromForm] ProdutoDto produtoDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                return BadRequest(new ResponseModel<ProdutoDto>
                {
                    status = false,
                    Mensagem = $"Dados inválidos: {string.Join("; ", errors.SelectMany(e => e.Errors))}"
                });
            }

            var response = await _produtoService.UpdateProdutoAsync(id, produtoDto);
            return response.status ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseModel<bool>>> DeleteProduto(int id)
        {
            var response = await _produtoService.DeleteProdutoAsync(id);
            return response.status ? Ok(response) : NotFound(response);
        }
    }
}
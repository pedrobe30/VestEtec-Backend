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

        [HttpPost("completo")]
        public async Task<ActionResult<ResponseModel<ProdutoDto>>> AddProdutoCompleto([FromBody] ProdutoCompletoDto produtoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseModel<ProdutoDto> { status = false, Mensagem = "Dados inválidos" });

            var response = await _produtoService.AddProdutoCompletoAsync(produtoDto);
            return response.status ? CreatedAtAction(nameof(GetProdutoById), new { id = response.Dados.IdProd }, response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseModel<ProdutoDto>>> UpdateProduto(int id, [FromBody] ProdutoDto produtoDto)
        {
            if (id != produtoDto.IdProd)
                return BadRequest(new ResponseModel<ProdutoDto> { status = false, Mensagem = "ID inconsistente" });

            if (!ModelState.IsValid)
                return BadRequest(new ResponseModel<ProdutoDto> { status = false, Mensagem = "Dados inválidos" });

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
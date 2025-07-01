// Controllers/ProdutoController.cs - ATUALIZADO PARA MÚLTIPLAS IMAGENS

using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Backend_Vestetec_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoService;
        private readonly ILogger<ProdutoController> _logger;

        public ProdutoController(IProdutoService produtoService, ILogger<ProdutoController> logger)
        {
            _produtoService = produtoService;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo produto com múltiplas imagens (máximo 4), tamanhos e quantidades
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseModel<ProdutoResponseDto>>> AddProdutoCompleto([FromForm] ProdutoCreateDto produtoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            // Validar preço
            if (produtoDto.Preco <= 0)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "O preço deve ser maior que zero."
                });
            }

            // Validar imagens
            if (produtoDto.Imagens == null || !produtoDto.Imagens.Any())
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "Pelo menos uma imagem é obrigatória."
                });
            }

            if (produtoDto.Imagens.Count > 4)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "Máximo de 4 imagens permitidas."
                });
            }

            // Validar tamanhos e quantidades
            if (string.IsNullOrWhiteSpace(produtoDto.TamanhosQuantidadesJson))
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "Os tamanhos e quantidades são obrigatórios."
                });
            }

            List<TamanhoQuantidadeDto> tamanhosQuantidades;
            try
            {
                var jsonLimpo = produtoDto.TamanhosQuantidadesJson.Trim();

                _logger.LogInformation("--> JSON Recebido para CRIAR produto: {JsonString}", jsonLimpo);

                if (jsonLimpo.StartsWith("\"") && jsonLimpo.EndsWith("\""))
                {
                    jsonLimpo = jsonLimpo.Substring(1, jsonLimpo.Length - 2);
                    _logger.LogInformation("--> JSON Após limpeza de aspas: {JsonString}", jsonLimpo);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                tamanhosQuantidades = JsonSerializer.Deserialize<List<TamanhoQuantidadeDto>>(jsonLimpo, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha no parsing do JSON ao criar produto.");
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = $"JSON inválido: {ex.Message}. Formato esperado: [{{'tamanho':'P','quantidade':10}}]"
                });
            }

            if (tamanhosQuantidades == null || !tamanhosQuantidades.Any())
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "Lista de tamanhos está vazia após parsing."
                });
            }

            // Validar cada item de tamanho/quantidade
            foreach (var item in tamanhosQuantidades)
            {
                if (string.IsNullOrWhiteSpace(item.Tamanho))
                {
                    return BadRequest(new ResponseModel<ProdutoResponseDto>
                    {
                        status = false,
                        Mensagem = "Todos os tamanhos devem ser preenchidos."
                    });
                }
                if (item.Quantidade < 0)
                {
                    return BadRequest(new ResponseModel<ProdutoResponseDto>
                    {
                        status = false,
                        Mensagem = "As quantidades não podem ser negativas."
                    });
                }
            }

            // Criar DTO completo
            var produtoCompleto = new ProdutoCompletoDto
            {
                Preco = produtoDto.Preco,
                IdCategoria = produtoDto.IdCategoria,
                IdModelo = produtoDto.IdModelo,
                IdTecido = produtoDto.IdTecido,
                Descricao = produtoDto.Descricao,
                Imagens = produtoDto.Imagens,
                TamanhosQuantidades = tamanhosQuantidades
            };

            _logger.LogInformation("Criando produto com {ImagensCount} imagens", produtoDto.Imagens.Count);

            var response = await _produtoService.AddProdutoCompletoAsync(produtoCompleto);

            if (!response.status || response.Dados == null)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(GetProdutoById), new { id = response.Dados.IdProd }, response);
        }

        /// <summary>
        /// Atualiza um produto existente com controle de múltiplas imagens
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResponseModel<ProdutoResponseDto>>> UpdateProduto(int id, [FromForm] ProdutoUpdateDto produtoDto)
        {
            if (id <= 0)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "ID do produto deve ser maior que zero."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            // Validar se teremos imagens suficientes após a atualização
            var imagensParaManter = produtoDto.ImagensParaManter?.Count ?? 0;
            var novasImagens = produtoDto.NovasImagens?.Count ?? 0;
            var totalImagens = imagensParaManter + novasImagens;

            if (totalImagens == 0)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "O produto deve ter pelo menos uma imagem."
                });
            }

            if (totalImagens > 4)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "Máximo de 4 imagens permitidas."
                });
            }

            List<TamanhoQuantidadeDto> tamanhosQuantidades;
            try
            {
                var jsonLimpo = produtoDto.TamanhosQuantidadesJson.Trim();

                _logger.LogInformation("--> JSON Recebido para ATUALIZAR produto: {JsonString}", jsonLimpo);

                if (jsonLimpo.StartsWith("\"") && jsonLimpo.EndsWith("\""))
                {
                    jsonLimpo = jsonLimpo.Substring(1, jsonLimpo.Length - 2);
                    _logger.LogInformation("--> JSON Após limpeza de aspas: {JsonString}", jsonLimpo);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                tamanhosQuantidades = JsonSerializer.Deserialize<List<TamanhoQuantidadeDto>>(jsonLimpo, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha no parsing do JSON ao atualizar produto.");
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = $"JSON inválido: {ex.Message}"
                });
            }

            if (tamanhosQuantidades == null || !tamanhosQuantidades.Any())
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "Lista de tamanhos está vazia após parsing."
                });
            }

            var produtoCompleto = new ProdutoUpdateCompletoDto
            {
                Preco = produtoDto.Preco,
                IdCategoria = produtoDto.IdCategoria,
                IdModelo = produtoDto.IdModelo,
                IdTecido = produtoDto.IdTecido,
                IdStatus = produtoDto.IdStatus,
                Descricao = produtoDto.Descricao,
                NovasImagens = produtoDto.NovasImagens,
                ImagensParaManter = produtoDto.ImagensParaManter,
                TamanhosQuantidades = tamanhosQuantidades
            };

            _logger.LogInformation("Atualizando produto {Id} - Imagens para manter: {Manter}, Novas imagens: {Novas}",
                id, imagensParaManter, novasImagens);

            var response = await _produtoService.UpdateProdutoAsync(id, produtoCompleto);
            return response.status ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Busca todos os produtos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<ProdutoResponseDto>>>> GetAllProdutos()
        {
            var response = await _produtoService.GetAllProdutosAsync();
            return response.status ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Busca produto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseModel<ProdutoResponseDto>>> GetProdutoById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ResponseModel<ProdutoResponseDto>
                {
                    status = false,
                    Mensagem = "ID do produto deve ser maior que zero."
                });
            }
            var response = await _produtoService.GetProdutoByIdAsync(id);
            return response.status ? Ok(response) : NotFound(response);
        }

        /// <summary>
        /// Busca produtos por categoria
        /// </summary>
        [HttpGet("categoria/{categoriaId}")]
        public async Task<ActionResult<ResponseModel<List<ProdutoResponseDto>>>> GetProdutosByCategoria(int categoriaId)
        {
            if (categoriaId <= 0)
            {
                return BadRequest(new ResponseModel<List<ProdutoResponseDto>>
                {
                    status = false,
                    Mensagem = "ID da categoria deve ser maior que zero."
                });
            }
            var response = await _produtoService.GetProdutosByCategoriaAsync(categoriaId);
            return response.status ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Deleta um produto
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseModel<bool>>> DeleteProduto(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ResponseModel<bool>
                {
                    status = false,
                    Mensagem = "ID do produto deve ser maior que zero.",
                    Dados = false
                });
            }
            var response = await _produtoService.DeleteProdutoAsync(id);
            return response.status ? Ok(response) : BadRequest(response);
        }
    }
}
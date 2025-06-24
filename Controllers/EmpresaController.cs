using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Vestetec_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresasController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    /// <summary>
    /// Obtém todas as empresas
    /// </summary>
    /// <returns>Lista de empresas</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetEmpresas()
    {
        var empresas = await _empresaService.GetAllAsync();
        return Ok(empresas);
    }

    /// <summary>
    /// Obtém uma empresa específica por ID
    /// </summary>
    /// <param name="id">ID da empresa</param>
    /// <returns>Dados da empresa</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmpresaDto>> GetEmpresa(int id)
    {
        var empresa = await _empresaService.GetByIdAsync(id);
        
        if (empresa == null)
        {
            return NotFound($"Empresa com ID {id} não encontrada.");
        }

        return Ok(empresa);
    }

    /// <summary>
    /// Obtém empresas formatadas para dropdown
    /// </summary>
    /// <returns>Lista simplificada de empresas</returns>
    [HttpGet("dropdown")]
    public async Task<ActionResult<IEnumerable<EmpresaDropdownDto>>> GetEmpresasForDropdown()
    {
        var empresas = await _empresaService.GetForDropdownAsync();
        return Ok(empresas);
    }

    /// <summary>
    /// Cadastra uma nova empresa
    /// </summary>
    /// <param name="empresaCreateDto">Dados da empresa a ser criada</param>
    /// <returns>Empresa criada</returns>
    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> PostEmpresa(EmpresaCreateDto empresaCreateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var empresa = await _empresaService.CreateAsync(empresaCreateDto);
        
        return CreatedAtAction(
            nameof(GetEmpresa), 
            new { id = empresa.IdEmpresa }, 
            empresa
        );
    }

    /// <summary>
    /// Atualiza uma empresa existente
    /// </summary>
    /// <param name="id">ID da empresa</param>
    /// <param name="empresaUpdateDto">Dados atualizados da empresa</param>
    /// <returns>Empresa atualizada</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<EmpresaDto>> PutEmpresa(int id, EmpresaUpdateDto empresaUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var empresa = await _empresaService.UpdateAsync(id, empresaUpdateDto);
        
        if (empresa == null)
        {
            return NotFound($"Empresa com ID {id} não encontrada.");
        }

        return Ok(empresa);
    }

    /// <summary>
    /// Deleta uma empresa
    /// </summary>
    /// <param name="id">ID da empresa</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmpresa(int id)
    {
        var result = await _empresaService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound($"Empresa com ID {id} não encontrada.");
        }

        return NoContent();
    }
}
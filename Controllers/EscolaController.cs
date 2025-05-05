using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Backend_Vestetec_App.Models;
using Dto.Escola;
using Alunos.Services;
using Backend_Vestetec_App.Controllers;
using Backend_Vestetec_App.Services;



namespace Backend_Vestetec_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EscolaController : ControllerBase
    {
        private readonly IEscolaService _escolaService;

        public EscolaController(IEscolaService escolaService)
        {
            _escolaService = escolaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EscolaDto>>> GetEscolas()
        {
            var escolas = await _escolaService.GetAllEscolasAsync();
            return Ok(escolas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EscolaDto>> GetEscola(int id)
        {
            var escola = await _escolaService.GetEscolaByIdAsync(id);
            
            if (escola == null)
                return NotFound();
                
            return Ok(escola);
        }
    }
}
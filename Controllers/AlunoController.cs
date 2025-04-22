using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Backend_Vestetec_App.Models;
using Dto.Aluno;
using Alunos.Services;
using Backend_Vestetec_App.Controllers;
using Backend_Vestetec_App.Services;

namespace Backend_Vestetec_App.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class AlunoController : ControllerBase
    {

        private readonly IAlunoInterface _alunoInterface;


        public AlunoController(IAlunoInterface alunoInterface)
        {
            _alunoInterface = alunoInterface;

        }




        [HttpPost("CriarAluno")]

        public async Task<ActionResult<ResponseModel<Aluno>>> AdicionarAluno(CriacaoAluno criacaoAluno)
        {

            var alunos = await _alunoInterface.AdicionarAluno(criacaoAluno);
            return Ok(alunos);

            
            
        }


    }
}
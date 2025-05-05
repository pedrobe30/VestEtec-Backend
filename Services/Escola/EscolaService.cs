using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dto.Escola;
using Dto.Aluno;
using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Services;



namespace Backend_Vestetec_App.Services
{
    public class EscolaService : IEscolaService
    {
        private readonly AppDbContext _context;

        public EscolaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EscolaDto>> GetAllEscolasAsync()
        {
            var escolas = await _context.Escolas
                .Select(e => new EscolaDto
                {
                    IdEsc = e.IdEscola,
                    nome_esc = e.NomeEsc
                })
                .OrderBy(e => e.nome_esc) // Order schools alphabetically
                .ToListAsync();

            return escolas;
        }

        public async Task<EscolaDto> GetEscolaByIdAsync(int id)
        {
            var escola = await _context.Escolas
                .Where(e => e.IdEscola == id)
                .Select(e => new EscolaDto
                {
                    IdEsc = e.IdEscola,
                    nome_esc = e.NomeEsc
                })
                .FirstOrDefaultAsync();

            return escola;
        }
    }
}
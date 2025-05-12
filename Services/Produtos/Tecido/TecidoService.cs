using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Services
{
    public class TecidoService : ITecidoService
    {
        private readonly AppDbContext _context;

        public TecidoService(AppDbContext context )
        {
            _context = context;
        }

        public async Task<IEnumerable<Tecido>> GetAllTecido()
        {
            return await _context.Tecidos.ToListAsync();
        }

        public async Task<Tecido> CreateTecido(Tecido tecido)
        {
            _context.Tecidos.Add(tecido);
            await _context.SaveChangesAsync();
            return tecido;
        }

        public async Task<Tecido> UpdateTecido(int id, Tecido tecido)
        {
            if (id != tecido.IdTecido)
            return null;

            _context.Entry(tecido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TecidoExists(id))
                return null;
                
                throw;
            }
             return tecido;
        }

        public async Task<bool> DeleteTecido(int id)
        {
            var tecido = await _context.Tecidos.FindAsync(id);
            if (tecido == null)
                return false;

            _context.Tecidos.Remove(tecido);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TecidoExists(int id)
        {
            return await _context.Tecidos.AnyAsync(c => c.IdTecido == id);
        }
    }

    
}

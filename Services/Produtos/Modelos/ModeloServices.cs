using Backend_Vestetec_App.Models;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Services
{
    public class ModeloService : IModeloService
    {
        private readonly AppDbContext _context;

        public ModeloService(AppDbContext context )
        {
            _context = context;
        }

        public async Task<IEnumerable<Modelo>> GetAllModelosAsync()
        {
            return await _context.Modelos.ToListAsync();
        }

        public async Task<Modelo> CreateModelo(Modelo modelo)
        {
            _context.Modelos.Add(modelo);
            await _context.SaveChangesAsync();
            return modelo;
        }

        public async Task<Modelo> UpdateModeloAsync(int id, Modelo modelo)
        {
            if (id != modelo.IdModelo)
            return null;

            _context.Entry(modelo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ModeloExists(id))
                return null;
                
                throw;
            }
             return modelo;
        }

        public async Task<bool> DeleteModelo(int id)
        {
            var modelo = await _context.Modelos.FindAsync(id);
            if (modelo == null)
                return false;

            _context.Modelos.Remove(modelo);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ModeloExists(int id)
        {
            return await _context.Modelos.AnyAsync(c => c.IdModelo == id);
        }
    }
}

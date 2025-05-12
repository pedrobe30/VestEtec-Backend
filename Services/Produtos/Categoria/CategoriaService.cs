using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend_Vestetec_App.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly AppDbContext _context;

        // Injeção de dependência do contexto do EF Core
        public CategoriaService(AppDbContext context)
        {
            _context = context;
        }

        // Busca todas as categorias do banco de dados de forma assíncrona
        public async Task<IEnumerable<Categoria>> GetAllCategoriasAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        // Busca uma categoria específica pelo ID
        public async Task<Categoria> GetCategoriaByIdAsync(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        // Cria uma nova categoria no banco de dados
        public async Task<Categoria> CreateCategoriaAsync(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return categoria;
        }

        // Atualiza uma categoria existente
        public async Task<Categoria> UpdateCategoriaAsync(int id, Categoria categoria)
        {
            // Verifica se o ID da categoria é o mesmo fornecido na rota
            if (id != categoria.IdCategoria)
                return null;

            // Marca a entidade como modificada no contexto
            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Se a categoria não existir mais, retorna null
                if (!await CategoriaExistsAsync(id))
                    return null;
                throw;
            }

            return categoria;
        }

        // Remove uma categoria do banco de dados
        public async Task<bool> DeleteCategoriaAsync(int id)
        {
            // Busca a categoria pelo ID
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return false;

            // Remove a categoria do contexto
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return true;
        }

        // Verifica se uma categoria existe no banco de dados
        public async Task<bool> CategoriaExistsAsync(int id)
        {
            return await _context.Categorias.AnyAsync(c => c.IdCategoria == id);
        }
    }
}
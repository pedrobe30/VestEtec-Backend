using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Vestetec_App.Services;

public class EmpresaService : IEmpresaService
{
    private readonly AppDbContext _context; // Substitua pelo seu DbContext específico
    
    public EmpresaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmpresaDto>> GetAllAsync()
    {
        var empresas = await _context.Set<Empresa>()
            .OrderBy(e => e.Nome)
            .ToListAsync();

        return empresas.Select(e => new EmpresaDto
        {
            IdEmpresa = e.IdEmpresa,
            Nome = e.Nome
        });
    }

    public async Task<EmpresaDto?> GetByIdAsync(int id)
    {
        var empresa = await _context.Set<Empresa>()
            .FirstOrDefaultAsync(e => e.IdEmpresa == id);

        if (empresa == null)
            return null;

        return new EmpresaDto
        {
            IdEmpresa = empresa.IdEmpresa,
            Nome = empresa.Nome
        };
    }

    public async Task<EmpresaDto> CreateAsync(EmpresaCreateDto empresaCreateDto)
    {
        var empresa = new Empresa
        {
            Nome = empresaCreateDto.Nome
        };

        _context.Set<Empresa>().Add(empresa);
        await _context.SaveChangesAsync();

        return new EmpresaDto
        {
            IdEmpresa = empresa.IdEmpresa,
            Nome = empresa.Nome
        };
    }

    public async Task<EmpresaDto?> UpdateAsync(int id, EmpresaUpdateDto empresaUpdateDto)
    {
        var empresa = await _context.Set<Empresa>()
            .FirstOrDefaultAsync(e => e.IdEmpresa == id);

        if (empresa == null)
            return null;

        empresa.Nome = empresaUpdateDto.Nome;
        
        await _context.SaveChangesAsync();

        return new EmpresaDto
        {
            IdEmpresa = empresa.IdEmpresa,
            Nome = empresa.Nome
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var empresa = await _context.Set<Empresa>()
            .FirstOrDefaultAsync(e => e.IdEmpresa == id);

        if (empresa == null)
            return false;

        _context.Set<Empresa>().Remove(empresa);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<EmpresaDropdownDto>> GetForDropdownAsync()
    {
        var empresas = await _context.Set<Empresa>()
            .OrderBy(e => e.Nome)
            .Select(e => new EmpresaDropdownDto
            {
                IdEmpresa = e.IdEmpresa,
                Nome = e.Nome
            })
            .ToListAsync();

        return empresas;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Set<Empresa>()
            .AnyAsync(e => e.IdEmpresa == id);
    }
}
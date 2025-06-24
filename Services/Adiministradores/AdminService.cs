// Services/AdminService.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Backend_Vestetec_App.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Backend_Vestetec_App.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private const string CODIGO_PRECISO_VALIDO = "0309";

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminResponseDto> CreateAdminAsync(AdminCreateDto adminDto)
        {
            // Validar código preciso
            if (adminDto.CodigoPreciso != CODIGO_PRECISO_VALIDO)
            {
                throw new ArgumentException("Código preciso inválido. Não é possível cadastrar o administrador.");
            }

            // Verificar se email já existe
            if (await EmailExistsAsync(adminDto.Email))
            {
                throw new ArgumentException("Email já está em uso por outro administrador.");
            }

            // Verificar se empresa existe
            var empresaExists = await _context.Empresas.AnyAsync(e => e.IdEmpresa == adminDto.IdEmpresa);
            if (!empresaExists)
            {
                throw new ArgumentException("Empresa não encontrada.");
            }

            var admin = new Admin
            {
                Email = adminDto.Email.ToLower(),
                Senha = HashPassword(adminDto.Senha),
                Nome = adminDto.Nome,
                IdEmpresa = adminDto.IdEmpresa,
                CodigoPreciso = adminDto.CodigoPreciso
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return await GetAdminResponseDto(admin);
        }

        public async Task<AdminResponseDto?> GetAdminByIdAsync(int id)
        {
            var admin = await _context.Admins
                .Include(a => a.IdEmpresaNavigation)
                .FirstOrDefaultAsync(a => a.IdAdm == id);

            return admin != null ? await GetAdminResponseDto(admin) : null;
        }

        public async Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync()
        {
            var admins = await _context.Admins
                .Include(a => a.IdEmpresaNavigation)
                .ToListAsync();

            var adminDtos = new List<AdminResponseDto>();
            foreach (var admin in admins)
            {
                adminDtos.Add(await GetAdminResponseDto(admin));
            }

            return adminDtos;
        }

        public async Task<AdminResponseDto?> UpdateAdminAsync(int id, AdminUpdateDto adminDto)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return null;

            // Verificar se email já existe (excluindo o admin atual)
            if (!string.IsNullOrEmpty(adminDto.Email) && await EmailExistsAsync(adminDto.Email, id))
            {
                throw new ArgumentException("Email já está em uso por outro administrador.");
            }

            // Verificar se empresa existe (se fornecida)
            if (adminDto.IdEmpresa.HasValue)
            {
                var empresaExists = await _context.Empresas.AnyAsync(e => e.IdEmpresa == adminDto.IdEmpresa.Value);
                if (!empresaExists)
                {
                    throw new ArgumentException("Empresa não encontrada.");
                }
            }

            // Atualizar apenas os campos fornecidos
            if (!string.IsNullOrEmpty(adminDto.Email))
                admin.Email = adminDto.Email.ToLower();

            if (!string.IsNullOrEmpty(adminDto.Senha))
                admin.Senha = HashPassword(adminDto.Senha);

            if (!string.IsNullOrEmpty(adminDto.Nome))
                admin.Nome = adminDto.Nome;

            if (adminDto.IdEmpresa.HasValue)
                admin.IdEmpresa = adminDto.IdEmpresa.Value;

            await _context.SaveChangesAsync();

            return await GetAdminResponseDto(admin);
        }

        public async Task<bool> DeleteAdminAsync(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return false;

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdminExistsAsync(int id)
        {
            return await _context.Admins.AnyAsync(a => a.IdAdm == id);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Admins.Where(a => a.Email.ToLower() == email.ToLower());

            if (excludeId.HasValue)
                query = query.Where(a => a.IdAdm != excludeId.Value);

            return await query.AnyAsync();
        }

        private async Task<AdminResponseDto> GetAdminResponseDto(Admin admin)
        {
            // Carregar empresa se não estiver carregada
            if (admin.IdEmpresaNavigation == null)
            {
                await _context.Entry(admin)
                    .Reference(a => a.IdEmpresaNavigation)
                    .LoadAsync();
            }

            return new AdminResponseDto
            {
                IdAdm = admin.IdAdm,
                Email = admin.Email,
                Nome = admin.Nome,
                IdEmpresa = admin.IdEmpresa,
                NomeEmpresa = admin.IdEmpresaNavigation?.Nome,
                DataCriacao = DateTime.Now // Você pode adicionar este campo ao modelo se necessário
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
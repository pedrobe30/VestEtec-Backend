// Controllers/AdminController.cs
using Backend_Vestetec_App.DTOs;
using Backend_Vestetec_App.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Vestetec_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Criar novo administrador
        /// </summary>
        /// <param name="adminDto">Dados do administrador</param>
        /// <returns>Administrador criado</returns>
        [HttpPost]
        public async Task<ActionResult<AdminResponseDto>> CreateAdmin([FromBody] AdminCreateDto adminDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var admin = await _adminService.CreateAdminAsync(adminDto);
                return CreatedAtAction(nameof(GetAdmin), new { id = admin.IdAdm }, admin);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obter administrador por ID
        /// </summary>
        /// <param name="id">ID do administrador</param>
        /// <returns>Dados do administrador</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminResponseDto>> GetAdmin(int id)
        {
            try
            {
                var admin = await _adminService.GetAdminByIdAsync(id);
                if (admin == null)
                    return NotFound(new { message = "Administrador não encontrado" });

                return Ok(admin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obter todos os administradores
        /// </summary>
        /// <returns>Lista de administradores</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminResponseDto>>> GetAllAdmins()
        {
            try
            {
                var admins = await _adminService.GetAllAdminsAsync();
                return Ok(admins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar administrador
        /// </summary>
        /// <param name="id">ID do administrador</param>
        /// <param name="adminDto">Dados para atualização</param>
        /// <returns>Administrador atualizado</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminResponseDto>> UpdateAdmin(int id, [FromBody] AdminUpdateDto adminDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedAdmin = await _adminService.UpdateAdminAsync(id, adminDto);
                if (updatedAdmin == null)
                    return NotFound(new { message = "Administrador não encontrado" });

                return Ok(updatedAdmin);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletar administrador
        /// </summary>
        /// <param name="id">ID do administrador</param>
        /// <returns>Resultado da operação</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAdmin(int id)
        {
            try
            {
                var deleted = await _adminService.DeleteAdminAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Administrador não encontrado" });

                return Ok(new { message = "Administrador deletado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Verificar se email já existe
        /// </summary>
        /// <param name="email">Email para verificar</param>
        /// <returns>Se o email existe</returns>
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _adminService.EmailExistsAsync(email);
                return Ok(new { emailExists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
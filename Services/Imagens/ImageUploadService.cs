// Services/ImageUploadService.cs
using Backend_Vestetec_App.Interfaces;

namespace Backend_Vestetec_App.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageUploadService> _logger;

        // Lista de extensões permitidas para validação
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
        // Tamanho máximo do arquivo (5MB)
        private const long MaxFileSize = 5 * 1024 * 1024;

        public ImageUploadService(IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile, string folderName = "produtos")
        {
            try
            {
                // Validar se o arquivo foi enviado
                if (imageFile == null || imageFile.Length == 0)
                    throw new ArgumentException("Nenhum arquivo foi enviado");

                // Validar tamanho do arquivo
                if (imageFile.Length > MaxFileSize)
                    throw new ArgumentException($"O arquivo é muito grande. Tamanho máximo permitido: {MaxFileSize / (1024 * 1024)}MB");

                // Validar extensão do arquivo
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                    throw new ArgumentException("Formato de arquivo não suportado. Use: JPG, JPEG, PNG, GIF ou WEBP");

                // Criar nome único para o arquivo para evitar conflitos
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // Definir o caminho completo onde o arquivo será salvo
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
                
                // Criar o diretório se ele não existir
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Salvar o arquivo no servidor
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Retornar o caminho relativo que será usado como URL
                var relativePath = Path.Combine("uploads", folderName, uniqueFileName).Replace("\\", "/");
                
                _logger.LogInformation($"Imagem salva com sucesso: {relativePath}");
                
                return "/" + relativePath; // Adiciona barra no início para formar URL completa
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload da imagem");
                throw;
            }
        }

        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                // Remove a barra inicial se existir
                var cleanPath = imagePath.TrimStart('/');
                var fullPath = Path.Combine(_environment.WebRootPath, cleanPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Imagem deletada: {imagePath}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar imagem: {imagePath}");
                return false;
            }
        }

        public bool ValidateImageFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return false;

            if (imageFile.Length > MaxFileSize)
                return false;

            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(fileExtension);
        }
    }
}
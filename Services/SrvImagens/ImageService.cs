using Backend_Vestetec_App.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;

namespace Backend_Vestetec_App.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor; // Adicionar esta linha

        // Lista de tipos MIME permitidos para imagens
        private readonly string[] _allowedMimeTypes =
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        // Lista de extensões permitidas
        private readonly string[] _allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".webp"
        };

        // Modificar o construtor para incluir IHttpContextAccessor
        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Valida se o arquivo enviado é uma imagem válida
        /// </summary>
        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Arquivo é nulo ou vazio");
                return false;
            }

            // Verifica o tipo MIME
            if (!_allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning($"Tipo MIME não permitido: {file.ContentType}");
                return false;
            }

            // Verifica a extensão do arquivo
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning($"Extensão não permitida: {extension}");
                return false;
            }

            // Verifica o tamanho do arquivo (limitamos a 3MB para coincidir com sua configuração)
            if (file.Length > 3 * 1024 * 1024) // 3MB
            {
                _logger.LogWarning($"Arquivo muito grande: {file.Length} bytes");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gera a URL absoluta para uma imagem
        /// </summary>
        private string GetAbsoluteImageUrl(string relativePath)
        {
            try
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    return baseUrl + relativePath;
                }

                // Fallback para desenvolvimento local
                return $"{relativePath}";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar URL absoluta");
                return relativePath; // Retorna relativa em caso de erro
            }
        }

        /// <summary>
        /// Salva a imagem no servidor e retorna o caminho ABSOLUTO
        /// </summary>
        public async Task<string> SaveImageAsync(IFormFile imageFile, string folderPath = "uploads/produtos")
        {
            try
            {
                _logger.LogInformation($"Iniciando salvamento da imagem: {imageFile.FileName}");

                // Valida o arquivo antes de processar
                if (!IsValidImageFile(imageFile))
                {
                    throw new ArgumentException("Arquivo de imagem inválido");
                }

                // Configurar WebRootPath se não estiver definido
                string webRootPath = _environment.WebRootPath;

                if (string.IsNullOrEmpty(webRootPath))
                {
                    // Fallback: usar o diretório do projeto + wwwroot
                    webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                    _logger.LogWarning($"WebRootPath estava nulo. Usando fallback: {webRootPath}");
                }

                // Normalizar o folderPath (remover barras no início/fim)
                folderPath = folderPath.Trim('/').Trim('\\');

                // Criar o caminho completo da pasta de uploads
                var uploadsFolder = Path.Combine(webRootPath, folderPath);

                _logger.LogInformation($"Pasta de destino: {uploadsFolder}");

                // Criar o diretório se não existir (incluindo diretórios pais)
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation($"Diretório criado: {uploadsFolder}");
                }

                // Gerar um nome único para o arquivo
                var fileExtension = Path.GetExtension(imageFile.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var fullFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                _logger.LogInformation($"Caminho completo do arquivo: {fullFilePath}");

                // Salvar o arquivo no servidor
                using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Criar o caminho relativo
                var relativePath = $"/{folderPath}/{uniqueFileName}".Replace("\\", "/");

                // Converter para URL absoluta


                _logger.LogInformation($"Imagem salva com sucesso: {relativePath}");

                return relativePath;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar imagem: {Message}", ex.Message);
                throw new Exception($"Erro ao salvar imagem: {ex.Message}");
            }
        }

        /// <summary>
        /// Deleta uma imagem do servidor
        /// </summary>
        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogWarning("Caminho da imagem está vazio");
                    return false;
                }

                // Configurar WebRootPath se não estiver definido
                string webRootPath = _environment.WebRootPath;

                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                }

                // Se for URL absoluta, extrair apenas o caminho relativo
                string relativePath;
                if (imagePath.StartsWith("http"))
                {
                    var uri = new Uri(imagePath);
                    relativePath = uri.AbsolutePath.TrimStart('/');
                }
                else
                {
                    relativePath = imagePath.TrimStart('/').TrimStart('\\');
                }

                var fullPath = Path.Combine(webRootPath, relativePath);

                _logger.LogInformation($"Tentando deletar imagem: {fullPath}");

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Imagem deletada com sucesso: {fullPath}");
                    return true;
                }

                _logger.LogWarning($"Arquivo não encontrado para deletar: {fullPath}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar imagem: {ImagePath}", imagePath);
                return false;
            }
        }
    }
}
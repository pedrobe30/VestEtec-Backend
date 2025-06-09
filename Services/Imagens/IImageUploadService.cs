// Interfaces/IImageUploadService.cs
namespace Backend_Vestetec_App.Interfaces
{
    public interface IImageUploadService
    {
        /// <summary>
        /// Faz upload de uma imagem e retorna o caminho onde foi salva
        /// </summary>
        /// <param name="imageFile">Arquivo de imagem enviado pelo usuário</param>
        /// <param name="folderName">Nome da pasta onde salvar (padrão: "produtos")</param>
        /// <returns>Caminho relativo da imagem salva</returns>
        Task<string> UploadImageAsync(IFormFile imageFile, string folderName = "produtos");

        /// <summary>
        /// Deleta uma imagem do servidor
        /// </summary>
        /// <param name="imagePath">Caminho da imagem a ser deletada</param>
        /// <returns>True se deletou com sucesso, False caso contrário</returns>
        bool DeleteImage(string imagePath);

        /// <summary>
        /// Valida se o arquivo enviado é uma imagem válida
        /// </summary>
        /// <param name="imageFile">Arquivo a ser validado</param>
        /// <returns>True se é válido, False caso contrário</returns>
        bool ValidateImageFile(IFormFile imageFile);
    }
}
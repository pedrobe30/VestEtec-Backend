namespace Backend_Vestetec_App.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, string folderPath = "uploads/produtos");
        bool DeleteImage(string imagePath);
        bool IsValidImageFile(IFormFile file);
    }
}

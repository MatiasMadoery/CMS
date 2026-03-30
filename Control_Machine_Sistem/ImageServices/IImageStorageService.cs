namespace Control_Machine_Sistem.Services
{
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);

        Task DeleteImageAsync(string imageUrl, string containerName);
    }
}
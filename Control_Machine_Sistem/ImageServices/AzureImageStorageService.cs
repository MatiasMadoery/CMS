using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models; // Necesario para PublicAccessType
using Microsoft.Extensions.Configuration;

namespace Control_Machine_Sistem.Services
{
    public class AzureImageStorageService : IImageStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureImageStorageService> _logger;

        public AzureImageStorageService(IConfiguration configuration, ILogger<AzureImageStorageService> logger)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            if (!string.IsNullOrEmpty(connectionString))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
            }

            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0) return null;

            if (_blobServiceClient == null)
                throw new Exception("El cliente de Azure Blob no está inicializado.");

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Generar nombre único
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                var blobHttpHeader = new BlobHttpHeaders { ContentType = file.ContentType };

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeader
                    });
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen al contenedor {ContainerName}", containerName);
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl, string containerName)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var uri = new Uri(imageUrl);
                string blobName = Path.GetFileName(uri.LocalPath);

                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la imagen {ImageUrl}", imageUrl);
            }
        }
    }
}
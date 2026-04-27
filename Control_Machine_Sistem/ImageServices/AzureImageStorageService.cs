using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models; // Necesario para PublicAccessType
using Microsoft.Extensions.Configuration;

namespace Control_Machine_Sistem.Services
{
    public class AzureImageStorageService : IImageStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureImageStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            if (!string.IsNullOrEmpty(connectionString))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0) return null;

            if (_blobServiceClient == null)
                throw new Exception("El cliente de Azure Blob no está inicializado.");

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // MODIFICACIÓN CRÍTICA:
                // Primero intentamos crear, pero si falla con 409, lo atrapamos y seguimos.
                try
                {
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 409)
                {
                    // Si el error es 409, significa que ya existe. No hacemos nada, está bien.
                    Console.WriteLine($"El contenedor {containerName} ya existía.");
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    var blobHttpHeader = new BlobHttpHeaders { ContentType = file.ContentType };

                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeader
                    });
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // Solo logueamos, no lanzamos (throw) para que no se caiga la página
                Console.WriteLine($"Error real en la subida: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteImageAsync(string imageUrl, string containerName)
        {
            if (string.IsNullOrEmpty(imageUrl) || _blobServiceClient == null) return;

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                var uri = new Uri(imageUrl);
                string blobName = Path.GetFileName(uri.LocalPath);

                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar imagen: {ex.Message}");
            }
        }
    }
}
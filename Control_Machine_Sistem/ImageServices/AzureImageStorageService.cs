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
            // 1. Validaciones básicas
            if (file == null || file.Length == 0 || _blobServiceClient == null) return null;

            try
            {
                // 2. Obtener referencia al contenedor y CREARLO si no existe
                // Usamos PublicAccessType.Blob para que las fotos se puedan ver en el navegador
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // 3. Generar un nombre único para el archivo (evita que se pisen)
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(fileName);

                // 4. Subir el flujo de datos (Stream)
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                // 5. Devolver la URL real (la de Azurite ahora, la de Azure después)
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir imagen: {ex.Message}");
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
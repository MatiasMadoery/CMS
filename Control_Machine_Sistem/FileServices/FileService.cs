namespace Control_Machine_Sistem.Services
{
    public class FileService
    {
        public static async Task<List<string>> SaveManualsAsync(List<IFormFile> archivos, string subfolder = "documentation/manuals")
        {
            List<string> urls = new List<string>();

            if (archivos == null || archivos.Count == 0)
                return urls;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{archivo.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    urls.Add($"/{subfolder}/{uniqueFileName}");
                }
            }

            return urls;
        }

        public static async Task<List<string>> SaveDocAsync(List<IFormFile> archivos, string subfolder = "documentation/machines")
        {
            List<string> urls = new List<string>();

            if (archivos == null || archivos.Count == 0)
                return urls;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{archivo.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    urls.Add($"/{subfolder}/{uniqueFileName}");
                }
            }

            return urls;
        }

        public static async Task<List<string>> SaveSpareKitsAsync(List<IFormFile> archivos, string subfolder = "documentation/spareKits")
        {
            List<string> urls = new List<string>();

            if (archivos == null || archivos.Count == 0)
                return urls;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{archivo.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    urls.Add($"/{subfolder}/{uniqueFileName}");
                }
            }

            return urls;
        }

        public static async Task<List<string>> SaveServiceSheetAsync(List<IFormFile> archivos, string subfolder = "documentation/serviceSheets")
        {
            List<string> urls = new List<string>();

            if (archivos == null || archivos.Count == 0)
                return urls;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{archivo.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(fileStream);
                    }

                    urls.Add($"/{subfolder}/{uniqueFileName}");
                }
            }

            return urls;
        }

        public static async Task DeleteManualFileAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "App_Data", "documentation", "manuals", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al eliminar el archivo en {filePath}", ex);
                }
            }
            await Task.CompletedTask;
        }

        public static async Task DeleteSpareKitFileAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "App_Data", "documentation", "spareKits", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al eliminar el archivo en {filePath}", ex);
                }
            }
            await Task.CompletedTask;
        }

        public static async Task DeleteDocumentationFileAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "App_Data", "documentation", "machines", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al eliminar el archivo en {filePath}", ex);
                }
            }

            await Task.CompletedTask;
        }
        public static async Task DeleteServiceSheetAsync(string fileUrl)
        {            
            var fileName = Path.GetFileName(fileUrl);           
            var rootPath = Directory.GetCurrentDirectory();          
            var filePath = Path.Combine(rootPath, "App_Data", "documentation", "serviceSheets", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {                    
                    throw new Exception($"Error al eliminar el archivo en {filePath}", ex);
                }
            }
            await Task.CompletedTask;
        }

    }
}

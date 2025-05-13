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


    }
}

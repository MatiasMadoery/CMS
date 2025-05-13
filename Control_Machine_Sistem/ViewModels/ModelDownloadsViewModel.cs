namespace Control_Machine_Sistem.ViewModels
{
    public class ModelDownloadsViewModel
    {
        public List<FileDownloadViewModel> Manuals { get; set; } = new();
        public List<FileDownloadViewModel> SpareKits { get; set; } = new();
    }
}

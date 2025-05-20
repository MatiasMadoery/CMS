using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.ViewModels
{
    public class ServiceDetailsViewModel
    {
        public Service? Service { get; set; }
        public List<DocumentationViewModel>? ServiceDocuments { get; set; }
    }
}

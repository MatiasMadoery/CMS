using Microsoft.AspNetCore.Mvc.Rendering;

namespace Control_Machine_Sistem.ViewModels
{
    public class QrCodeIndexViewModel
    {
        public List<SelectListItem> Clients { get; set; }
        public List<SelectListItem> Machines { get; set; }
    }
}

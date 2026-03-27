using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.ViewModels
{
    public class SoldItemViewModel
    {
        public Pager<Machine> SoldMachines { get; set; }
        public Pager<Accessory> SoldAccessories { get; set; }
        public string ActiveTab { get; set; } = "machines";
    }
}

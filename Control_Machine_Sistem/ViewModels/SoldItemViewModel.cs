using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.ViewModels
{
    public class SoldItemViewModel
    {
        public List<Machine> SoldMachines { get; set; } = new List<Machine>();
        public List<Accessory> SoldAccessories { get; set; } = new List<Accessory>();
        public string ActiveTab { get; set; } = "machines";
    }
}

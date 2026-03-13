using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.ViewModels
{
    public class StockViewModel
    {
        public Pager<Machine>? Machines { get; set; }
        public Pager<Accessory>? Accessories { get; set; }
        public string ActiveTab { get; set; } = "machines"; // Para controlar qué pestaña se ve
    }
}

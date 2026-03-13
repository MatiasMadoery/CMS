namespace Control_Machine_Sistem.ViewModels
{
    public class SoldItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
        public DateTime? Date { get; set; }
        public string ItemType { get; set; } // "Machine" o "Accessory"
    }
}

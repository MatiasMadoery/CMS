namespace Control_Machine_Sistem.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? PurchaseDate { get; set; }
    }
}

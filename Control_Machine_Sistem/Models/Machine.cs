namespace Control_Machine_Sistem.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }

        public ICollection<Service>? Services { get; set; } = new List<Service>();
        public ICollection<Manual>? Manuals { get; set; } = new List<Manual>();
    }
}

namespace Control_Machine_Sistem.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Dni { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Province { get; set; }        
        public string? Country { get; set; }     
        
        public ICollection<Machine>? Machines { get; set; } = new List<Machine>();
    }
}

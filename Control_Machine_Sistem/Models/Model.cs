namespace Control_Machine_Sistem.Models
{
    public class Model
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Manual { get; set; }

        public ICollection<Machine>? Machines { get; set; } = new List<Machine>();
    }
}

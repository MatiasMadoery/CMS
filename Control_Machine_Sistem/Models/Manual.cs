namespace Control_Machine_Sistem.Models
{
    public class Manual
    {
        public int Id { get; set; }
        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }
        public string? ManualUrl { get; set; }
    }
}

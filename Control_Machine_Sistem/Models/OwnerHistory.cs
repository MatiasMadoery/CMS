namespace Control_Machine_Sistem.Models
{
    public class OwnerHistory
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public Machine? Machine { get; set; }
        public string? PreviousOwner { get; set; }
        public DateTime ChangeDate { get; set; }
    }

}

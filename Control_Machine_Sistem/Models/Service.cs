﻿namespace Control_Machine_Sistem.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string? Description { get; set; }
    }
}

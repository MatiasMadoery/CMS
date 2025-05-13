using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class OtherMaintenance
    {
        public int Id { get; set; }
        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }

        [Display(Name = "Mantenimiento/Horas")]
        public string? WorkHours { get; set; }

        [Display(Name = "Fecha")]
        public DateTime? ServiceDate { get; set; }

        [Display(Name = "N° Orden")]
        public string? OrderNumber { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observations { get; set; }
    }
}

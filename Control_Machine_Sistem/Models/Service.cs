using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_Machine_Sistem.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int? MachineId { get; set; }
        public Machine? Machine { get; set; }

        [Display(Name = "Hora Programada")]
        public int? ServiceHour { get; set; }
        [Required(ErrorMessage = "Debe ingresar las Horas Reales")]
        [Display(Name = "Horas Reales")]
        public int? WorkHours { get; set; }
        [Required(ErrorMessage = "Debe ingresar la Fecha del Service")]
        [Display(Name = "Fecha")]
        public DateTime? ServiceDate { get; set; }
        [Required(ErrorMessage = "Debe ingresar un N° de Orden")]
        [Display(Name = "N° Orden")]
        public string? OrderNumber { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observations { get; set; }

        [NotMapped]
        [Display(Name = "Planilla Service")]
        public IEnumerable<IFormFile>? ServiceSheet { get; set; }

        [Display(Name = "Planilla Service")]
        public List<string> ServiceSheetUrls { get; set; } = new List<string>();

    }
}

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_Machine_Sistem.Models
{
    public class Machine
    {
        public int Id { get; set; }
        [Display(Name = "Cliente")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [Required(ErrorMessage = "Debe seleccionar un modelo.")]
        [Display(Name = "Modelo")]
        public int? ModelId { get; set; }
        public Model? Model { get; set; }
        [Required(ErrorMessage = "Debe ingresar el Número de Chasis.")]
        [Display(Name = "Número de chasis")]
        public string? ChasisNumber { get; set; }
        [Required(ErrorMessage = "Debe ingresar el Número de Motor.")]

        [Display(Name = "Número de motor")]
        public string? EngineNumber { get; set; }
        [Required(ErrorMessage = "Debe ingresar la Fecha de Entrega Técnica.")]
        [Display(Name = "Fecha de entrega técnica")]        
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Fecha vencimiento garantía")]
        public DateTime? WarrantyExpirationDate { get; set; }



        //Nuevas Propiedades Para Stock
        [Required]
        [Display(Name = "Año de fabricación")]
        public int? ManufactureYear { get; set; }
        [Required]
        [Display(Name = "Número de serie")]
        public string? SerialNumber { get; set; }
        [Display(Name = "Horas de uso")]
        public int? UserHours { get; set; }
        [Required]
        [Display(Name = "Ubicación")]
        public int? UbicationId { get; set; }
        public Ubication? Ubication { get; set; }

        //
        [NotMapped]
        [Display(Name = "Documentación")]
        public IEnumerable<IFormFile>? Documentations { get; set; }

        [Display(Name = "Documentación")]
        public List<string> DocUrls { get; set; } = new List<string>();

        public ICollection<OwnerHistory>? OwnerHistories { get; set; }

        public ICollection<Service>? Services { get; set; }

        public ICollection<OtherMaintenance>? OtherMaintenances { get; set; }

    }
}

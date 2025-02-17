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

        [Display(Name = "Modelo")]
        public int? ModelId { get; set; }
        public Model? Model { get; set; }

        [Display(Name = "Número de chasis")]
        public string? ChasisNumber { get; set; }

        [Display(Name = "Número de motor")]
        public string? EngineNumber { get; set; }

        [Display(Name = "Fecha de entrega")]        
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Fecha vencimiento garantia")]
        public DateTime? WarrantyExpirationDate { get; set; }

        [NotMapped]        
        public IEnumerable<IFormFile>? Documentations { get; set; }
        [Display(Name = "Documentación")]
        public List<string> DocUrls { get; set; } = new List<string>();


    }
}
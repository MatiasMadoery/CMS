using Control_Machine_Sistem.Models.Control_Machine_Sistem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_Machine_Sistem.Models
{
    public class Accessory
    {
        public int Id { get; set; }


        // --- NUEVA RELACIÓN CON EL CATÁLOGO (AccessoryModel) ---
        [Required(ErrorMessage = "Debe seleccionar un modelo de accesorio del catálogo.")]
        [Display(Name = "Modelo de Accesorio")]
        public int AccessoryModelId { get; set; }

        [ForeignKey("AccessoryModelId")]
        public virtual AccessoryModel? AccessoryModel { get; set; }

        // --- PROPIEDADES DE CONTROL DE STOCK (Se quedan aquí) ---

        [Display(Name = "Fecha de Venta")]
        public DateTime? SaleDate { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        // --- RELACIÓN CON UBICACIÓN ---
        [Required(ErrorMessage = "Debe asignar una ubicación al accesorio")]
        [Display(Name = "Sucursal")]
        public int UbicationId { get; set; }

        [ForeignKey("UbicationId")]
        public virtual Ubication? Ubication { get; set; }

        // --- PROPIEDADES PARA FOTOS ---
        [NotMapped]
        [Display(Name = "Fotos del Accesorio")]
        public IEnumerable<IFormFile>? ImageFiles { get; set; }

        [Display(Name = "URLs de Fotos")]
        public List<string> ImageUrls { get; set; } = new List<string>();


        // Nota: Quitamos "Model", "LoadCapacity" y "CategoryId" de aquí porque
        // ahora se manejan a través de AccessoryModel.
    }
}
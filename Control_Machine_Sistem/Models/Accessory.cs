using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_Machine_Sistem.Models
{
    public class Accessory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Modelo/Descripción")]
        public string? Model { get; set; }

        [Required(ErrorMessage = "Debe ingresar la capacidad de carga.")]
        [Range(0, 50000, ErrorMessage = "Ingrese un valor válido")]
        [Display(Name = "Capacidad de Carga (kg)")]
        public double LoadCapacity { get; set; }

        [Display(Name = "Fecha de Venta")]
        public DateTime? SaleDate { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // --- RELACIÓN CON UBICACIÓN ---

        [Required(ErrorMessage = "Debe asignar una ubicación al accesorio")]
        [Display(Name = "Sucursal")]
        public int? UbicationId { get; set; }

        [ForeignKey("UbicationId")]
        public virtual Ubication? Ubication { get; set; }

        // --- PROPIEDADES PARA FOTOS Y DOCUMENTACIÓN (Igual que en Machine) ---

        [NotMapped]
        [Display(Name = "Fotos del Accesorio")]
        public IEnumerable<IFormFile>? ImageFiles { get; set; }

        [Display(Name = "URLs de Fotos")]
        public List<string> ImageUrls { get; set; } = new List<string>();

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Accessory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [Display(Name = "Modelo/Descripción")]
        public string? Model { get; set; }

        [Required(ErrorMessage = "Debe ingresar la capacidad de carga.")]
        [Display(Name = "Capacidad de Carga (kg)")]
        public double LoadCapacity { get; set; }

        [Display(Name = "Fecha de Venta")]
        public DateTime? SaleDate { get; set; }

        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}

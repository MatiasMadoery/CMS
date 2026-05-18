using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    namespace Control_Machine_Sistem.Models
    {
        public class AccessoryModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "El modelo es obligatorio.")]
            [StringLength(100)]
            [Display(Name = "Modelo")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe ingresar la capacidad de carga.")]
            [Range(0, 50000, ErrorMessage = "Ingrese un valor válido")]
            [Display(Name = "Capacidad de Carga (kg)")]
            public double LoadCapacity { get; set; }

            // --- RELACIÓN CON CATEGORÍA ---
            [Required(ErrorMessage = "El producto es obligatorio.")]
            [Display(Name = "Producto")]
            public int CategoryId { get; set; }

            [Display(Name = "Producto")]
            public virtual Category? Category { get; set; }

            // --- RELACIÓN INVERSA CON EL STOCK ---
            // Un modelo de catálogo puede tener muchas unidades físicas en stock
            public virtual ICollection<Accessory> Accessories { get; set; } = new List<Accessory>();
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Ubication
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
        [Display(Name = "Sucursal")]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        // Relación con Máquinas
        public ICollection<Machine>? Machines { get; set; }
    }
}

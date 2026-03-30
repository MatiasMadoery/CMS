using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Ubication
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre/ubicación de la sucursal es obligatorio.")]
        [Display(Name = "Sucursal")]
        public string Name { get; set; } = string.Empty;


        // Relación con Máquinas
        public ICollection<Machine>? Machines { get; set; }

        //Relación con Accesorios
        public ICollection<Accessory>? Accessories { get; set; }

    }
}

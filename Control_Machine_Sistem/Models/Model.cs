using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_Machine_Sistem.Models
{
    public class Model
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Modelo es obligatorio.")]
        [Display(Name = "Modelo")]
        public string? Name { get; set; }

        [NotMapped]
        [Display(Name = "Manuales")]
        public IEnumerable<IFormFile>? Manuals { get; set; }

        [NotMapped]
        [Display(Name = "Consumibles Services")]
        public IEnumerable<IFormFile>? SpareKits { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        [Display(Name = "Categoría")]
        public Category? Category { get; set; }

        [Display(Name = "Manuales")]
        public List<string> ManualUrls { get; set; } = new List<string>();
        [Display(Name = "Consumibles Services")]
        public List<string> SpareKitsUrls { get; set; } = new List<string>();
        public ICollection<Machine>? Machines { get; set; } = new List<Machine>();

    }
}

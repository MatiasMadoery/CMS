

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_Machine_Sistem.Models
{
    public class Model
    {
        public int Id { get; set; }
        [Required]

        [Display(Name = "Modelo")]
        public string? Name { get; set; }
        [NotMapped]
        public IEnumerable<IFormFile>? Manuals { get; set; }
        public List<string> ManualUrls { get; set; } = new List<string>();
        public ICollection<Machine>? Machines { get; set; } = new List<Machine>();
    }
}

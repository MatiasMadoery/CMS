using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name="Producto")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Model> Models { get; set; } = new List<Model>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public CategoryType Type { get; set; } = CategoryType.Machine;

        public ICollection<Model> Models { get; set; } = new List<Model>();
    }
}
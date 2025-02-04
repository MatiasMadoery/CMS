using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Rol { get; set; }
    }
}

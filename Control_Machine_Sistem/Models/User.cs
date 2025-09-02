using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        [StringLength(20, ErrorMessage = "El nombre no pede tener mas de 20 caracteres.")]
        public string? Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mail es obligatorio.")]
        [EmailAddress(ErrorMessage = "El mail debe tener un formato correcto.")]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 15 caracteres.")]
        public string? Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string? Rol { get; set; } = string.Empty;
    }
}

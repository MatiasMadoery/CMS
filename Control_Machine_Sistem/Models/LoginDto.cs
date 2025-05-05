using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(20, ErrorMessage = "El nombre no pede tener mas de 20 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 15 caracteres.")]
        public string? Password { get; set; } = string.Empty;
    }
}

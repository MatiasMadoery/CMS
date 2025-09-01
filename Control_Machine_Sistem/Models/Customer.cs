using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe ingresar la Razón Social.")]
        [Display(Name = "Razón Social")]
        public string? Name { get; set; }

        
        [Display(Name = "Persona de contacto")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Debe ingresar el CUIT.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El CUIT debe contener solo números.")]
        public string? Cuit { get; set; }

        [Required(ErrorMessage = "Debe ingresar el Teléfono")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Debe ingresar el Email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Dirección")]
        public string? Address { get; set; }

        [Display(Name = "Ciudad")]
        public string? City { get; set; }

        [Display(Name = "Código postal")]
        public string? PostalCode { get; set; }

        [Display(Name = "Provincia")]
        public string? Province { get; set; }

        [Display(Name = "País")]
        public string? Country { get; set; }     
        
        public ICollection<Machine>? Machines { get; set; } = new List<Machine>();

        [Display(Name = "Cliente")]
        public string FullName => $"{Name} {LastName}";
    }
}

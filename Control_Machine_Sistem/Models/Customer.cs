using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string? Name { get; set; }

        [Display(Name = "Apellido")]
        public string? LastName { get; set; }

        public string? Cuit { get; set; }

        [Display(Name = "Telefono")]
        public string? Phone { get; set; }
        public string? Email { get; set; }

        [Display(Name = "Direccion")]
        public string? Address { get; set; }

        [Display(Name = "Ciudad")]
        public string? City { get; set; }

        [Display(Name = "Codigo postal")]
        public string? PostalCode { get; set; }

        [Display(Name = "Provincia")]
        public string? Province { get; set; }

        [Display(Name = "Pais")]
        public string? Country { get; set; }     
        
        public ICollection<Machine>? Machines { get; set; } = new List<Machine>();

        public string FullName => $"{Name} {LastName}";
    }
}

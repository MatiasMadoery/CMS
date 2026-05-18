using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public enum CategoryType
    {
        [Display(Name = "MAQUINARIA")]
        Machine = 1,

        [Display(Name = "ACCESORIOS")]
        Accessory = 2
    }
}
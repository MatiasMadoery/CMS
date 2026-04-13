using System.ComponentModel.DataAnnotations;

namespace Control_Machine_Sistem.Models
{
    public enum CategoryType
    {
        [Display(Name = "MODELOS")]
        Machine = 1,

        [Display(Name = "ACCESORIOS")]
        Accessory = 2
    }
}
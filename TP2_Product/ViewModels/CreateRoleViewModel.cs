using System.ComponentModel.DataAnnotations;

namespace TP_Product.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = string.Empty;
    }
}
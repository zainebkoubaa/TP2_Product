using System.ComponentModel.DataAnnotations;

namespace TP_Product.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Nom")]
        public string CategoryName { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TP_Product.Models;

namespace TP_Product.ViewModels
{
    public class CreateViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; }

        [Display(Name = "Prix en dinar :")]
        public float Price { get; set; }

        [Required(ErrorMessage = "Le prix est obligatoire")]
        [RegularExpression(@"^\d+[Dd][Tt]\d{3}$", ErrorMessage = "Format obligatoire : 9DT000 (exemple : 5DT900)")]
        [Display(Name = "Prix unitaire")]
        public string PriceText { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantité en unité :")]
        public int QteStock { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required]
        [Display(Name = "Image :")]
        public IFormFile ImagePath { get; set; }
    }
}
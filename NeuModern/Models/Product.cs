using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NeuModern.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Size is required")]
        public string Size { get; set; }
        [Required(ErrorMessage = "StockQunatity is required")]
        [Range(0, 250)]
        public int StockQuantity { get; set; }
        [Required]

        public string Brand { get; set; }


        //[Required(ErrorMessage = "Discount percentage is required")]
        //[Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal Discount { get; set; }

        [Required(ErrorMessage = "Offer price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Offer price must be a positive number")]
        public decimal OfferPrice { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime DateTime { get; set; }


        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [ValidateNever]

        public List<ProductImage> ProductImages { get; set; }

        public Product()
        {
            Discount = OfferPrice;

        }
    }
}

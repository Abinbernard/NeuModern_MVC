using System.ComponentModel.DataAnnotations;

namespace NeuModern.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }
        [Required]
        public int MinAmount { get; set; }
        public int DiscountAmount { get; set; }
        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        public string CouponCode { get; set; }
        [Range(1, 100, ErrorMessage = "Discount Percentage should be between 1-100")]
        public double? DiscountPercentage { get; set; }


        public string? IsValid { get; set; }



        public DiscountType CouponType { get; set; }

    public enum DiscountType
    {
        Percentage,
        Amount
    }

}
}

namespace NeuModern.Models.ViewModel
{
    public class CouponVM
    {
        public Coupon Coupon { get; set; }
        public IEnumerable<ApplicationUser> ApplicationUser { get; set; }
    }
}

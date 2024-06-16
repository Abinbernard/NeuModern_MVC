namespace NeuModern.Models.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set;}
        public OrderHeader OrderHeader { get; set;}
        //public decimal OrderTotal { get; set;}
        public IEnumerable<Coupon> CouponList { get; set;}
        public MultipleAddress MultipleAddress { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}

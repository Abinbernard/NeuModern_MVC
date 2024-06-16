namespace NeuModern.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IProductImageRepository ProductImage { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IWishListRepository WishList { get; }
        ICouponRepository Coupon { get; }
        IOfferRepository Offer { get; }
        IMultipleAddressRepository MultipleAddress { get; }
        
        //ISalesReportRepository SalesReport { get; }

        
        void Save();
    }
}






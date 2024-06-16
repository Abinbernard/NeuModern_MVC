using NeuModern.IRepository;
using NeuModern.Models;

namespace NeuModern.Repository.IRepository
{
    public interface IWishListRepository : IRepository<WishList>
    {
        void Update(WishList obj); 
    }
}

using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class WishListRepository : Repository<WishList>,IWishListRepository
    {

        private ApplicationDbContext _db;
        public WishListRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(WishList obj)
        {
            _db.WishLists.Update(obj);
        }
    }
}


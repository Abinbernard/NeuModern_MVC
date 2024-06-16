using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class CouponRepository : Repository<Coupon>, ICouponRepository
    {
        private ApplicationDbContext _db;
        public CouponRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Coupon obj)
        {
            _db.Coupons.Update(obj);
        }
    }
}



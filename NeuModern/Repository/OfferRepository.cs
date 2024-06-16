using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class OfferRepository : Repository<Offer>, IOfferRepository
    {
        private ApplicationDbContext _db;
        public OfferRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Offer obj)
        {
            _db.Offers.Update(obj);
        }
    }
}



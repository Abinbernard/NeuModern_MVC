
using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class MultipleAddressRepository : Repository<MultipleAddress>, IMultipleAddressRepository
    {
        private ApplicationDbContext _db;
        public MultipleAddressRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(MultipleAddress obj)
        {
            _db.MultipleAddresses.Update(obj);
        }
    }
}




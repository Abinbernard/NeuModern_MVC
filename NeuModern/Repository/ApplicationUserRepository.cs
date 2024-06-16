using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser obj)
        {
            var applicatiionUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == obj.Id);
            if (applicatiionUser != null)
            {
                applicatiionUser.PhoneNumber = obj.PhoneNumber;
                applicatiionUser.Name = obj.Name;
                applicatiionUser.StreetAddress = obj.StreetAddress;
                applicatiionUser.City = obj.City;
                applicatiionUser.State = obj.State;
                applicatiionUser.PostalCode = obj.PostalCode;
                if (obj.Wallet != null)
                {
                    applicatiionUser.Wallet = obj.Wallet;
                }
            }
        }
    }
}


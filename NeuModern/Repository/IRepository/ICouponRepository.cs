using NeuModern.Models;
using NeuModern.IRepository;

namespace NeuModern.Repository.IRepository
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        void Update(Coupon obj);
    }
}





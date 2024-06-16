using NeuModern.Models;
using NeuModern.IRepository;

namespace NeuModern.Repository.IRepository
{
    public interface IOfferRepository : IRepository<Offer>
    {
        void Update(Offer obj);
    }
}




using NeuModern.Models;
using NeuModern.IRepository;

namespace NeuModern.Repository.IRepository
{
    public interface IMultipleAddressRepository : IRepository<MultipleAddress>
    {
        void Update(MultipleAddress obj);
    }
}




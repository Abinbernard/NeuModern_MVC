using NeuModern.Models;
using NeuModern.IRepository;

namespace NeuModern.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
    }
}




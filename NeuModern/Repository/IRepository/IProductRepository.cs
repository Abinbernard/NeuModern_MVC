using NeuModern.Models;
using NeuModern.IRepository;

namespace NeuModern.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
    }
}

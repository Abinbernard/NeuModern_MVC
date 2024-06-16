using NeuModern.Models;
using NeuModern.IRepository;

namespace NeuModern.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        void Update(ApplicationUser obj);
    }
}





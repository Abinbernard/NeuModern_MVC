using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;
using System.Linq.Expressions;

namespace NeuModern.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}



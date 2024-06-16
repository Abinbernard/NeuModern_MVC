using NeuModern.Areas.Identity.Data;
using NeuModern.Models;
using NeuModern.Repository.IRepository;

namespace NeuModern.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDDb != null)
            {
                orderFromDDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string payementIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(payementIntentId))
            {
                orderFromDb.PaymentIntentId = payementIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}



using Walmart.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Walmart.Model.Models;
using WalmartWeb.DataAccess;

namespace Walmart.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeaderFromDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderHeaderFromDb.PaymentStatus = paymentStatus;
                }
            }

        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderHeaderFromDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderHeaderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderHeaderFromDb.PaymentIntentId = paymentIntentId;
                orderHeaderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walmart.Model.Models
{
    public class OrderVM
    {

        public OrderHeader OrderHeader { get; set; }

        public IEnumerable<OrderDetail> OrderDetail { get; set; }

        public RazorPayOptionModel RazorPayOptionModel { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walmart.Model.Models
{
    public class RazorPayOptionModel
    {
        public string Key { get; set; }

        public double AmountInSubUnits { get; set; }

        public string Currency { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageLogUrl { get; set; }

        public string OrderId { get; set; }

        public string ProdileName { get; set; }

        public string ProfileContact { get; set; }

        public string ProfileEmail { get; set; }

        public Dictionary<string,string> Notes { get; set; }

        public int OrderHeaderId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models
{
    public class CSVCost
    {
        public int ResourceId { get; set; }
        public string meterCategory { get; set; }
        public string meterName { get; set; }
        public string ProductName { get; set; }
        public string resourceGroupName { get; set; }
        public string billingCurrency { get; set; }
        public decimal costInBillingCurrency { get; set; }
        //public string meterName { get; set; }
    }
}

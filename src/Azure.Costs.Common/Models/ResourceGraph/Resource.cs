using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Costs.Common.Models.Rest;

namespace Azure.Costs.Common.Models.ResourceGraph
{
    public class RootResource
    {
        public int count { get; set; }
        public Resource[] data { get; set; }
    }
    public class Resource
    {

        //public string Resource { get; set; }

        public string resourceGroup { get; set; }
        public string location { get; set; }
        public string subscriptionId { get; set; }
        public string skuName { get; set; }

        // value is 
        // ,"tags":{"octopus-environment":"infrastructure","octopus-project":"infrastructure-operat etc
        //public string tags { get; set; }
        public string Details { get; set; }

        
    }
}

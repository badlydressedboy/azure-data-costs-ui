using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEstateOverview.Models.Rest;

namespace DbMeta.Ui.Wpf.Models.Rest
{
    public class RootVNet
    {
        public List<VNet> value { get; set; }

    }

    public class VNet : PortalResource
    {        
        public VNetProperties properties { get; set; }
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string resourceGroup { get; set; }
        public string location { get; set; }
        public Subscription Subscription { get; set; }
        public decimal TotalCostBilling { get; set; }
        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();
    }

    public class VNetProperties
    {
        public List<VNetSubnet> subnets { get; set; }
        public List<VNetPeering> virtualNetworkPeerings { get; set; }
        public bool enableDdosProtection { get; set; }  
    }
    public class VNetSubnet
    {

    }
    public class VNetPeering
    {

    }
}

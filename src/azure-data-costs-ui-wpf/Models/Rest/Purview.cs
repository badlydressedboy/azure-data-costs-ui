using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEstateOverview.Models.Rest;
using DbMeta.Ui.Wpf.Models.Rest;

namespace DbMeta.Ui.Wpf.Models.Rest
{
    public class RootPurview
    {
        public List<Purview> value { get; set; }

    }
        
    public class Purview : PortalResource
    {
        public string name { get; set; }
        public string location { get; set; }
        public string id { get; set; }
        public string resourceGroup { get; set; }
        public PurviewProperties properties { get; set; }
        public Subscription Subscription { get; set; }
        public decimal TotalCostBilling { get; set; }
        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();

        public PurviewSku sku { get; set; }
    }

    public class PurviewProperties
    {
        public string friendlyName { get; set; }
        public string publicNetworkAccess { get; set; }
        public string managedResourceGroupName { get; set; }
        //public string friendlyName { get; set; }
        public PurviewPropertiesManagedResources managedResources { get; set; }
}
    public class PurviewSku
    {
        public string name { get; set; }
        public int capacity { get; set; }
    }
    public class PurviewPropertiesManagedResources
    {
        public string resourceGroup { get; set; }
        public string storageAccount { get; set; }

    }

}

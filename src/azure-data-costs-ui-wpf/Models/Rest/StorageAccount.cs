using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMeta.Ui.Wpf.Models.Rest
{
    public class RootStorageAccount
    {
        public List<StorageAccount> value { get; set; }
    }
    public class StorageAccount
    {
        public StorageAccountSku sku { get; set; }  
        public StorageAccountProperties properties { get; set; }  
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string resourceGroup { get; set; }
        public string location { get; set; }
        public Subscription Subscription { get; set; }
        public decimal TotalCostBilling { get; set; }
        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();

    }

    public class StorageAccountSku
    {
        public string name { get; set; }
        public string tier { get; set; }
    }

    public class StorageAccountProperties
    {
        public bool allowBlobPublicAccess { get; set; }
        public string accessTier { get; set; }
        public DateTime creationTime { get; set; }
        public string primaryLocation { get; set; }
        

    }
}

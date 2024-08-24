using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class RootSubscription
    {
        public List<Subscription> value { get; set; }
    }
    public class Subscription : ObservableObject
    {
       
        public string subscriptionId { get; set; }
        public string displayName { get; set; }
        public string tenantId { get; set; }

        private bool readCosts;
        public bool ReadCosts
        {
            get => readCosts;
            set
            {
                SetProperty(ref readCosts, value);

                // costs can only be read if the corresponding objects are read too
                if (readCosts)
                {
                    ReadObjects = true;
                }
            }
        }
        private bool readObjects;
        public bool ReadObjects
        {
            get => readObjects;
            set
            {
                SetProperty(ref readObjects, value);

                // costs can only be read if the corresponding objects are read too

                if (value == false)
                {
                    ReadCosts = false;
                }
            }
        }

        public bool HasEverGotSqlServers { get; set; }  
        public List<RestSqlServer> SqlServers { get; set; } = new List<RestSqlServer>();
        public List<DataFactory> DataFactories { get; set; } = new List<DataFactory>();
        public List<ResourceCost> ResourceCosts { get; set; } = new List<ResourceCost>();
        public List<StorageAccount> StorageAccounts { get; set; } = new List<StorageAccount>();
        public List<VNet> VNets { get; set; } = new List<VNet>();
        public List<VM> VMs { get; set; } = new List<VM>();
        public List<Purview> Purviews { get; set; } = new List<Purview>();
        public List<Cosmos> Cosmos { get; set; } = new List<Cosmos>();
        public List<Fabric> FabricCapacities { get; set; } = new List<Fabric>();

        public DateTime LastCostGetDate { get; set; }
       
        private string costsErrorMessage;
        public string CostsErrorMessage
        {
            get => costsErrorMessage;
            set => SetProperty(ref costsErrorMessage, value);
        }
        public bool NeedsNewCosts()
        {
            if (LastCostGetDate < DateTime.Now.AddHours(-1))
            {
                return true;
            }
            return false;
        }
        public Subscription(string subscriptionId)
        {       
            this.subscriptionId = subscriptionId;
            readCosts = true;
            readObjects = true; 
        }
    }
}

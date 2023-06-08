using CommunityToolkit.Mvvm.ComponentModel;
using DataEstateOverview.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMeta.Ui.Wpf.Models.Rest
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
            set => SetProperty(ref readCosts, value);
        }
        private bool readObjects;
        public bool ReadObjects
        {
            get => readObjects;
            set => SetProperty(ref readObjects, value);
        }


        public List<RestSqlServer> SqlServers { get; set; } = new List<RestSqlServer>();
        public List<DataFactory> DataFactories { get; set; } = new List<DataFactory>();
        public List<ResourceCost> ResourceCosts { get; set; } = new List<ResourceCost>();
        public List<StorageAccount> StorageAccounts { get; set; } = new List<StorageAccount>();
        public List<VNet> VNets { get; set; } = new List<VNet>();
        public List<VM> VMs { get; set; } = new List<VM>();

        public DateTime LastCostGetDate { get; set; }
        public string CostsErrorMessage { get; set; }
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

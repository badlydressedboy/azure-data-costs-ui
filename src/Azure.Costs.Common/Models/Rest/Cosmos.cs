using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



namespace Azure.Costs.Common.Models.Rest
{
    public class RootCosmos
    {
        public List<Cosmos> value { get; set; }

    }
        
    public class Cosmos : PortalResource, INotifyPropertyChanged
    {
        public string name { get; set; }
        public string location { get; set; }
        
        public string id { get; set; }
        public string resourceGroup { get; set; }
        public CosmosProperties properties { get; set; }
        public Subscription Subscription { get; set; }
        private decimal _totalCostBilling;
        public decimal TotalCostBilling
        {
            get { return _totalCostBilling; }
            set
            {
                _totalCostBilling = value;
                OnPropertyChanged("TotalCostBilling");
            }
        }
        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();

        public CosmosSku sku { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class CosmosProperties
    {
        public string provisioningState { get; set; }
        public string publicNetworkAccess { get; set; }
        public string EnabledApiTypes { get; set; }
        
        public bool enableAutomaticFailover { get; set; }
        public bool enableMultipleWriteLocations { get; set; }
        public bool enablePartitionKeyMonitor { get; set; }
        public bool isVirtualNetworkFilterEnabled { get; set; }
        public List<PrivateEndPointConnection> privateEndPointConnections { get; set; }

        public bool enableAnalyticalStorage { get; set; }
        public bool enablePartitionMerge { get; set; }
        public bool enableBurstCapacity { get; set; }
        public string defaultIdentity { get; set; }

        public List<CosmosLocation> writeLocations { get; set; }
        public List<CosmosLocation> readLocations { get; set; }
        public List<CosmosLocation> locations { get; set; }
        public List<CosmosFailoverPolicy> failoverPolicies { get; set; }
        //public Dictionary<string, string> capabilities { get; set; }
    }
    public class CosmosSku
    {
        public string name { get; set; }
        public int capacity { get; set; }
    }
    public class CosmosPropertiesManagedResources
    {
        public string resourceGroup { get; set; }
        public string storageAccount { get; set; }

    }
    public class CosmosLocation
    {
        public string locationName { get; set; }
        public string documentEndpoint { get; set; }
        public string provisioningState { get; set; }
        public int failoverPriority { get; set; }
        public bool isZoneRedundant { get; set; }

    }
    public class CosmosFailoverPolicy
    {
        public string locationName { get; set; }
        public int failoverPriority { get; set; }

    }
    public class CosmosBackupPolicy
    {
        public string type { get; set; }
        public CosmosPeriodicModeProperties periodicModeProperties { get; set; }

    }
    public class CosmosPeriodicModeProperties
    {
        public string backupStorageRedundancy { get; set; }
        public int backupIntervalInMinutes { get; set; }
        public int backupRetentionIntervalInHours { get; set; }

    }
}

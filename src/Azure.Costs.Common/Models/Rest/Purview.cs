using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



namespace Azure.Costs.Common.Models.Rest
{
    public class RootPurview
    {
        public List<Purview> value { get; set; }

    }
        
    public class Purview : PortalResource, INotifyPropertyChanged
    {
        public string name { get; set; }
        public string location { get; set; }
        public string id { get; set; }
        public string resourceGroup { get; set; }
        public PurviewProperties properties { get; set; }
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

        public PurviewSku sku { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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

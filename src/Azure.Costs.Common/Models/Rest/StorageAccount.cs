using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Azure.Costs.Common.Models.Rest
{
    public class RootStorageAccount
    {
        public List<StorageAccount> value { get; set; }
    }
    public class StorageAccount : PortalResource, INotifyPropertyChanged
    {
        
        public StorageAccountSku sku { get; set; }  
        public StorageAccountProperties properties { get; set; }  
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string resourceGroup { get; set; }
        public string location { get; set; }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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

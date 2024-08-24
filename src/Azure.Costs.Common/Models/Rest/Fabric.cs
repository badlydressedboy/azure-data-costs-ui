using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



namespace Azure.Costs.Common.Models.Rest
{
    public class RootFabric
    {
        public List<Fabric> value { get; set; }

    }
        
    public class Fabric : PortalResource, INotifyPropertyChanged
    {
        public string name { get; set; }
        public string location { get; set; }
        public FabricSku sku { get; set; }
        public string id { get; set; }
        public string resourceGroup { get; set; }
        public FabricProperties properties { get; set; }
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

        public Fabric()
        {
            
        }
    }

    public class FabricSku
    {
        public string name { get; set; }
        public string tier { get; set; }
    }
    public class FabricProperties
    {
        public string state { get; set; }
    }

}

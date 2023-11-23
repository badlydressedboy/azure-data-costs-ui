using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataEstateOverview.Models.Rest;

namespace DbMeta.Ui.Wpf.Models.Rest
{
    public class RootVNet
    {
        public List<VNet> value { get; set; }

    }

    public class VNet : PortalResource, INotifyPropertyChanged
    {        
        public VNetProperties properties { get; set; }
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

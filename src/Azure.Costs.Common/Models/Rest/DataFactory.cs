using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Azure.Costs.Common.Models.Rest;

namespace Azure.Costs.Common.Models.Rest
{
    /* {"id":"/subscriptions/a5be5e3e-da5c-45f5-abe9-9591a51fccfa/resourceGroups/ot-dev-mi-rg-02/providers/Microsoft.DataFactory/factories/ot-dev-mi-adf-we-02"
     * ,"name":"ot-dev-mi-adf-we-02"
     * ,"type":"Microsoft.DataFactory/factories"
     * ,"location":"westeurope"
     * ,"identity":{"principalId":"762e34e6-26ba-4905-9832-94e2eeee7dbe","tenantId":"4b48b40b-d4d4-47ab-a97e-aa6ab59e21a7","type":"SystemAssigned"}
     * ,"createdTime":"2022-01-26T09:45:45.9806227Z"
     * ,"changedTime":"2022-10-18T13:05:31.8938739Z"
     * ,"tags":{"deployed_by":"terraform","domain":"mi_platform","environment":"dev","repo":"mi.platform","type":"data_factory"}}
     * 
     */

    public class DataFactoryRoot
    {
        public List<DataFactory> value { get; set; }    
    }
    public class DataFactory : PortalResource, INotifyPropertyChanged
    {
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string resourceGroup { get; set; }
        //public string subscriptionid { get; set; }
        public Subscription Subscription { get; set; }        
        public DateTime createdTime { get; set; }
        public DateTime changedTime { get; set; }

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
}

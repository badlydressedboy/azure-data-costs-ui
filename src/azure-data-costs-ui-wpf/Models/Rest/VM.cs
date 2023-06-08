using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMeta.Ui.Wpf.Models.Rest
{
    public class RootVM
    {
        public List<VM> value { get; set; } 
    }
    public class VM
    {
        public VMProperties properties { get; set; }
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string resourceGroup { get; set; }
        public string location { get; set; }
        public Subscription Subscription { get; set; }
        public decimal TotalCostBilling { get; set; }
        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();
    }
    public class VMProperties
    {
        public string vmId { get; set; }
        public VMHardwareProfile hardwareProfile { get; set; }
        public VMOSProfile osProfile { get; set; }
        public DateTime timeCreated { get; set; }
    }
    public class VMHardwareProfile
    {
        public string vmSize { get; set; }
    }
    public class VMOSProfile
    {
        public string computerName { get; set; }
    }
}

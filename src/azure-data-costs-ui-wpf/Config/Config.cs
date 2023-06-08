using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMeta.Ui.Wpf.Config
{
    public class Config
    {
        public List<ConfigSubscription> Subscriptions { get; set; }
    }

    public class ConfigSubscription
    {
        public string Name { get; set; }    
        public bool ReadObjects { get; set; }
        public bool ReadCosts { get; set; }

        public ConfigSubscription()
        {
            ReadObjects = true;
            ReadCosts = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMeta.Ui.Wpf.Models.Rest;

namespace Azure.Costs.Ui.Wpf.Models.Rest
{
    public class RootTenant
    {
        public List<Tenant> value { get; set; }
    }
    public class Tenant
    {

        public string defaultDomain { get; set; }
    }
}

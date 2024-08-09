using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Azure.Costs.Common.Models.Rest
{
    public class RootTenant
    {
        public List<Tenant> value { get; set; }
    }
    public class Tenant
    {

        public string defaultDomain { get; set; }
        public string displayName { get; set; }
    }
}

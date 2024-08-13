using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class RootPvScan
    {
        public List<PvScan> value { get; set; }
    }
    public class PvScan
    {
        public string name { get; set; }
        public PvScanProperties Properties { get; set; }

    }
    public class PvScanProperties
    {
        public string databaseName { get; set; }
        public string serverEndpoint { get; set; }
    }
}

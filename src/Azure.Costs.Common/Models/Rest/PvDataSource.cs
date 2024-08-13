using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class RootPvDataSource
    {
        public List<PvDataSource> value { get; set; }
    }
    public class PvDataSource
    {
        public string name { get; set; }
        public string kind { get; set; }
        public PvDataSourceProperties properties { get; set; }
        public List<PvScan> Scans { get; set; }

    }

    public class PvDataSourceProperties
    {
        public string serverEndpoint { get; set; }
        public string resourceId { get; set; }
        public string resourceName { get; set; }
    }
}

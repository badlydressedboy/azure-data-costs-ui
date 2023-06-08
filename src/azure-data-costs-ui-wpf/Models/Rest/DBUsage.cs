using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.Rest
{
    public class DBUsageRoot
    {
        public List<DBUsage> value { get; set; }
    }
    public class DBUsage
    {
        public string name { get; set; }
        public DBUsageProperties properties { get; set; }
    }
    public class DBUsageProperties
    {
        public string displayName { get; set; }
        public decimal currentValue { get; set; }
        public decimal limit { get; set; }
        public string unit { get; set; }
    }
}

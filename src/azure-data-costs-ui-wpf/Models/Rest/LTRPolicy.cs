using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.Rest
{
    public class LTRPolicy
    {
        public string id { get; set; }
        public LTRPolicyProperties properties { get; set; }
    }
    public class LTRPolicyProperties
    {
        public string monthlyRetention { get; set; }
        public long weekOfYear { get; set; }
        public string weeklyRetention { get; set; }
        public string yearlyRetention { get; set; }
    }
}

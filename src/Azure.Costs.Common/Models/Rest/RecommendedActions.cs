using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class Advisor
    {
        public string name { get; set; }
        public AdvisorProperties properties { get; set; }
    }
    
    public class AdvisorProperties
    {
        public List<RecommendedAction> recommendedActions { get; set; }
    }
    public class RecommendedAction
    {
        public string name { get; set; }
        public RecommendedActionProperties properties { get; set; }        
    }
    public class RecommendedActionProperties
    {
        public string recommendationReason { get; set; }
        public DateTime validSince { get; set; }
        public int score { get; set; }
        public RecommendedActionImplementationDetails implementationDetails { get; set; }
    }
    public class RecommendedActionImplementationDetails
    {
        public string method { get; set; }
        public string script { get; set; }
    }
}

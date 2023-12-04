using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class RootServiceTierAdvisor
    {
        public ServiceTierAdvisor[] value { get; set; }
    }
    public class ServiceTierAdvisor
    {
        public string name { get; set; }
  
        public ServiceTierAdvisorProperties properties { get; set; }
    }

    public class ServiceTierAdvisorProperties
    {
        public DateTime observationPeriodStart { get; set; }

        public DateTime observationPeriodEnd { get; set; }

        public long minDtu { get; set; }
        public long avgDtu { get; set; }
        public long maxDtu { get; set; }
        public decimal maxSizeInGB { get; set; }

        public string usageBasedRecommendationServiceLevelObjective { get; set; }

        public string databaseSizeBasedRecommendationServiceLevelObjective { get; set; }

        public string disasterPlanBasedRecommendationServiceLevelObjective { get; set; }

        public string overallRecommendationServiceLevelObjective { get; set; }

        public string confidence { get; set; }

    }
    }

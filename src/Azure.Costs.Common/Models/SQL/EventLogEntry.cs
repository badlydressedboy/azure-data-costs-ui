using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.SQL
{
    public class EventLogEntry
    {
        public string Database { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventCategory { get; set; }
        public string EventType { get; set; }
        public string EventSubType { get; set; }
        public long Severity { get; set; }
        public long EventCount { get; set; }
        public string Description { get; set; }
    }
}

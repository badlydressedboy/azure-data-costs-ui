using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public class Session
    {
        public long SessionId { get; set; }
        public string LoginName { get; set; }
        public string IpAddress { get; set; }
        public string HostName { get; set; }
        public string ProgramName { get; set; }

        public string SqlText { get; set; }

        public string Status { get; set; }

        public DateTime LastRequestStartTime { get; set; }
    }
}

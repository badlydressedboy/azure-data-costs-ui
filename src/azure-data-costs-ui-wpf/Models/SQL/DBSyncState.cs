using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public class DBSyncState
    {

        public string IsLocal { get; set; }
        public string IsPrimaryReplica { get; set; }
        public string PartnerServer { get; set; }
        public string PartnerDatabase { get; set; }
        public string SyncState { get; set; }
        public string SyncHealth { get; set; }
        public string DBState { get; set; }
        public DateTime LastSentTime { get; set; }
        public long LogSendRate { get; set; }
        public string SecondaryLagSeconds { get; set; }
        public string Role { get; set; }
        public string SecondaryAllowConnect { get; set; }

    }
}

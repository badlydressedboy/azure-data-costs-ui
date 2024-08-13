using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class RootPvTrigger
    {
        public List<PvTrigger> value { get; set; }
    }
    public class PvTrigger
    {
        public string name { get; set; }
        public PvTriggerProperties properties { get; set; }
    }

    public class PvTriggerProperties
    {

        public string scanLevel { get; set; }
        public PvTriggerRecurrence recurrence { get; set; }
    }
    public class PvTriggerRecurrence
    {
        public string frequency { get; set; }
        public int interval { get; set; }

        public PvTriggerRecurrenceSchedule schedule { get; set; }
    }
    public class PvTriggerRecurrenceSchedule
    {
        public string weekDays { get; set; }
        public string monthDays { get; set; }
        //public string minutes { get; set; }
        //public string hours { get; set; }
    }
}

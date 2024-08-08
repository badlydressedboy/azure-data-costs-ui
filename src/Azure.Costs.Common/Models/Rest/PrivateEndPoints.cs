using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class PrivateEndPointConnection
    {
        public string id {  get; set; }
        public PrivateEndPointConnectionProperties properties { get; set; }
    }
    public class PrivateEndPointConnectionProperties
    {
        public string id { get; set; }
        public PrivateLinkServiceConnectionState privateLinkServiceConnectionState { get; set; }
    }
    public class PrivateLinkServiceConnectionState
    {
        public string status { get; set; }
        public string actionsRequired { get; set; }
    }
}

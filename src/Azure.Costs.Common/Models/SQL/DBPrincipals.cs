using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.SQL
{
    public class DBPrincipal
    {

        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string AuthType { get; set; }
    }
}

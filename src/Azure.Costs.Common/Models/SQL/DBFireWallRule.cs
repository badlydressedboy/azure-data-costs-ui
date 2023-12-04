using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public class FireWallRule
    {

        public string Name { get; set; }
        public string StartIP { get; set; }
        public string EndIP { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

    }
}

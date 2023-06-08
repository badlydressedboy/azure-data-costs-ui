using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public class ServerGroup
    {
        public string Name { get; set; }
        public List<AzServer> AzServers { get; set; } = new List<AzServer>();

        public List<AzDB> MasterDbPropsList { get; set; } = new List<AzDB>();


    }


}

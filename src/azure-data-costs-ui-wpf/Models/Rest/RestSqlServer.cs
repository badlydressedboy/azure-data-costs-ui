﻿using DataEstateOverview.Models.SQL;
using DbMeta.Ui.Wpf.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.Rest
{
    public class RestSqlServer
    {
        public string id { get; set; }
        public string name { get; set; }

        //public string subscriptionid { get; set; }
        public Subscription Subscription { get; set; }

        public string resourceGroup { get; set; }
        public string location { get; set; }      
        
        // sql class used to get lower level detail then available from rest api
        public AzServer AzServer { get; set; }

        public List<RestSqlDb> Dbs { get; set; } = new List<RestSqlDb>();
    }

    public class RootRestSqlServer
    {
        public RestSqlServer[] value { get; set; }
    }
}

﻿using DataEstateOverview.Models.SQL;
using DbMeta.Ui.Wpf.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.Rest
{
    public class RootRestSqlDb
    {
        public RestSqlDb[] value { get; set; }
    }

    public class RestSqlDb
    {
        // rest is king and sql is its child that uses queries to get low level meta data
        public AzDB AzDB { get; set; } = new AzDB();
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string serverName { get; set; }

        //public string subscriptionid { get; set; }
        public Subscription Subscription { get; set; }

        public string resourceGroup { get; set; }
        public RestSqlDbProps properties { get; set; }
        public ServiceTierAdvisor serviceTierAdvisor { get; set; }
        public LTRPolicyProperties lTRPolicyProperties { get; set; }

        public VulnerabilityScanProperties latestVulnerabilityScanProperties { get; set; }
        public string VulnerabilityScanError { get; set; }
        public string RecommendationsError { get; set; }

        public int advisorRecomendationCount { get; set; }
        public string advisorRecomendationDetails { get; set; } = "";

        private decimal _currentDbSizeBytes;
        public decimal currentDbSizeBytes
        {
            get { return _currentDbSizeBytes; }
            set
            {
                _currentDbSizeBytes = value;
                currentDbSizeUsedPc = _currentDbSizeLimitBytes > 0 ? _currentDbSizeBytes / _currentDbSizeLimitBytes * 100 : 0;
                currentDbSizeGb = (double)_currentDbSizeBytes / 1024.0 / 1024.0 / 1024.0;
            }
        }
        public double currentDbSizeGb { get; set; }

        private decimal _currentDbSizeLimitBytes;
        public decimal currentDbSizeLimitBytes
        {
            get { return _currentDbSizeLimitBytes; }
            set
            {
                _currentDbSizeLimitBytes = value;
                currentDbSizeUsedPc = _currentDbSizeLimitBytes>0 ? _currentDbSizeBytes / _currentDbSizeLimitBytes * 100 : 0;
                currentDbSizeLimitGb = (double)_currentDbSizeLimitBytes / 1024.0 / 1024.0 / 1024.0;
            }
        }
        public double currentDbSizeLimitGb { get; set; }
        public decimal currentDbSizeUsedPc { get; set; }


        private decimal _currentAllocatedDbSizeBytes;
        public decimal currentAllocatedDbSizeBytes
        {
            get { return _currentAllocatedDbSizeBytes; }
            set
            {
                _currentAllocatedDbSizeBytes = value;
                currentAllocatedDbSizeUsedPc = _currentAllocatedDbSizeLimitBytes > 0 ? _currentAllocatedDbSizeBytes / _currentAllocatedDbSizeLimitBytes * 100 : 0;
                currentAllocatedDbSizeGb = (double)_currentAllocatedDbSizeBytes / 1024.0 / 1024.0 / 1024.0;
            }
        }
        public double currentAllocatedDbSizeGb { get; set; }

        private decimal _currentAllocatedDbSizeLimitBytes;
        public decimal currentAllocatedDbSizeLimitBytes
        {
            get { return _currentAllocatedDbSizeLimitBytes; }
            set
            {
                _currentAllocatedDbSizeLimitBytes = value;
                currentAllocatedDbSizeUsedPc = _currentAllocatedDbSizeLimitBytes > 0 ? _currentAllocatedDbSizeBytes / _currentAllocatedDbSizeLimitBytes * 100 : 0;
                currentAllocatedDbSizeLimitGb = (double)_currentAllocatedDbSizeLimitBytes / 1024.0 / 1024.0 / 1024.0;
            }
        }
        public double currentAllocatedDbSizeLimitGb { get; set; }
        public decimal currentAllocatedDbSizeUsedPc { get; set; }



        public decimal dtu_consumption_percent { get; set; }
        public long sessions_count { get; set; }

        public decimal physical_data_read_percent { get; set; }
        public decimal log_write_percent { get; set; }
        public long storage { get; set; } 
        public decimal storage_percent { get; set; }
        public decimal workers_percent { get; set; }
        public decimal sessions_percent { get; set; }
        public long dtu_limit { get; set; }
        public long dtu_used { get; set; }
        public decimal sqlserver_process_core_percent { get; set; }
        public decimal sqlserver_process_memory_percent { get; set; }
        public decimal tempdb_data_size { get; set; }
        public decimal tempdb_log_size { get; set; }
        public decimal tempdb_log_used_percent { get; set; }
        private long _allocated_data_storage;
        public long allocated_data_storage { 
            get { return _allocated_data_storage; }
            set { _allocated_data_storage = value;
                allocated_data_storage_gb = (double)_allocated_data_storage / 1024.0 / 1024.0 / 1024.0;
            } 
        }
        public double allocated_data_storage_gb { get; set; }

        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();

        public decimal TotalCostBilling { get; set; }

    }
    public class RestSqlDbProps
    {
        public string status { get; set; }
        public string kind { get; set; }
        public long maxSizeBytes { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime earliestRestoreDate { get; set; }

        public string currentServiceObjectiveName { get; set; }
        public string defaultSecondaryLocation { get; set; }

        public bool zoneRedundant { get; set; }
        public string currentBackupStorageRedundancy { get; set; }

    }
}

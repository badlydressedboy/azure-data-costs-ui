using Azure.Costs.Common.Models.SQL;
using Azure.Costs.Common.Models.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class RootRestSqlDb
    {
        public RestSqlDb[] value { get; set; }
    }
    public abstract class PortalResource
    {
        protected static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public string PortalResourceUrl { get; set; }

        protected string _tagsString;
        public string TagsString { get { return _tagsString; }
            set
            {
                
                // populate TagsList
                try
                {
                    var tmp = value.Replace("{", "").Replace("}","");

                    _tagsString = tmp;
                    TagsStringMultiLine = tmp.Replace(",", ",\n");  

                    var tags = tmp.Split(',');
                    TagsList = tags.ToList();
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        public string TagsStringMultiLine { get; set; }

        public List<string> TagsList { get; set; } = new List<string>();

        //protected string _tags ;
        public dynamic tags {
            get { return TagsString; }
            set {
                TagsString = value.ToString();
            }
        }
    }
    public class RestSqlDb : PortalResource, INotifyPropertyChanged
    {
        // rest is king and sql is its child that uses queries to get low level meta data
        public AzDB AzDB { get; set; } = new AzDB();
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string serverName { get; set; }
        public string CostsErrorMessage { get; set; } // this is for convenience to display in UI, value taken from subscription costs error message

        //public string subscriptionid { get; set; }
        public Subscription Subscription { get; set; }
        public ElasticPool ElasticPool { get; set; }    
        public string resourceGroup { get; set; }
        public RestSqlDbProps properties { get; set; }
        public ServiceTierAdvisor serviceTierAdvisor { get; set; }
        public LTRPolicyProperties lTRPolicyProperties { get; set; }

        public VulnerabilityScanProperties latestVulnerabilityScanProperties { get; set; }
        public string VulnerabilityScanError { get; set; }
        public string RecommendationsError { get; set; }

        //public int advisorRecommendationCount { get; set; }
        public string advisorRecommendationDetails { get; set; } = "";
        
        protected int advisorRecommendationCount;
        
        public int AdvisorRecommendationCount {
            get { return advisorRecommendationCount; }
            set
            {
                advisorRecommendationCount = value;
                if (value == 0)
                {
                    AdvisorRecommendationSummary = $"No Advisor Recommendations";
                }
                else if (value == 1)
                {
                    AdvisorRecommendationSummary = $"1 Advisor Recommendation";
                }
                else
                {
                    AdvisorRecommendationSummary = $"{value} Advisor Recommendations";
                }
            }
        }

        public string AdvisorRecommendationSummary { get; set; } = "";

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
        public decimal _maxDtuUsed { get; set; }
        public decimal MaxDtuUsed
        {
            get { return _maxDtuUsed; }
            set
            {
                _maxDtuUsed = value;
                OnPropertyChanged("MaxDtuUsed");
               
            }
        }

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

        private decimal _overSpendFromMaxPc;
        public decimal OverSpendFromMaxPc // 100 - max(dtu/cpu use)
        {
            get { return _overSpendFromMaxPc; }
            set
            {
                _overSpendFromMaxPc = value;
                OverSpendFromMaxPcString = value.ToString();
                OnPropertyChanged("OverSpendFromMaxPc");
            }
        }
        private string _overSpendFromMaxPcString = "?";
        public string OverSpendFromMaxPcString // 100 - max(dtu/cpu use)
        {
            get { return _overSpendFromMaxPcString; }
            set
            {
                _overSpendFromMaxPcString = value;
                OnPropertyChanged("OverSpendFromMaxPcString");
            }
        }

        private decimal _potentialSavingAmount;
        public decimal PotentialSavingAmount 
        {
            get { return _potentialSavingAmount; }
            set
            {
                _potentialSavingAmount = value;
                PotentialSavingAmountString = value.ToString("N0");
                OnPropertyChanged("PotentialSavingAmount");
            }
        }
        private string _potentialSavingAmountString = "?";
        public string PotentialSavingAmountString 
        {
            get { return _potentialSavingAmountString; }
            set
            {
                _potentialSavingAmountString = value;
                OnPropertyChanged("PotentialSavingAmountString");
            }
        }
        private string _potentialSavingDescription = "None";
        public string PotentialSavingDescription
        {
            get { return _potentialSavingDescription; }
            set
            {
                _potentialSavingDescription = value;
                OnPropertyChanged("PotentialSavingDescription");
            }
        }

        public double allocated_data_storage_gb { get; set; }

        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();

        private decimal _totalCostBilling;
        public decimal TotalCostBilling { 
            get { return _totalCostBilling; }
            set
            {
                _totalCostBilling = value;
                OnPropertyChanged("TotalCostBilling");
            }
        }

        public DateTime MetricsFromTime { get; set; }
        public DateTime MetricsToTime { get; set; }


        public bool _isRestQueryBusy { get; set; }
        public bool IsRestQueryBusy
        {
            get { return _isRestQueryBusy; }
            set
            {
                _isRestQueryBusy = value;
                OnPropertyChanged("IsRestQueryBusy");
            }
        }

        public bool RequestMetricsHistory { get; set; } = false;

        public bool GotMetricsHistory { get; set; } = false;

        protected int _metricsHistoryMinutes;

        public int MetricsHistoryMinutes
        {
            get { return _metricsHistoryMinutes; }
            set
            {
                _metricsHistoryMinutes = value;
                OnPropertyChanged("MetricsHistoryMinutes");
                OnPropertyChanged("MetricsHistoryTimeString");
            }
        }

        protected int _metricsHistoryDays = 14;

        public int MetricsHistoryDays
        {
            get { return _metricsHistoryDays; }
            set
            {
                _metricsHistoryDays = value;
                OnPropertyChanged("MetricsHistoryDays");                
            }
        }
        protected string _metricsErrorMessage;

        public string MetricsErrorMessage
        {
            get { return _metricsErrorMessage; }
            set
            {
                _metricsErrorMessage = value;
                OnPropertyChanged("MetricsErrorMessage");                
            }
        }
        protected string _spendAnalysisStatus;

        public string SpendAnalysisStatus
        {
            get { return _spendAnalysisStatus; }
            set
            {
                _spendAnalysisStatus = value;
                OnPropertyChanged("SpendAnalysisStatus");
            }
        }

        //protected bool _spendAnalysisStatus;

        public bool HasAdvisorRecommendations
        {
            get {
                if (advisorRecommendationCount > 0) return true;
                return false; 
            }            
        }


        public bool IsElaticPoolMember
        {
            get { 
                if(ElasticPool == null)
                {
                    return false;
                }
                return true;
            }
          
        }

        public bool IsSynapse
        {
            get
            {
                if (properties.currentServiceObjectiveName.Contains("DW"))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsVCore
        {
            get
            {
                if (properties.currentServiceObjectiveName.Contains("GP_"))
                {
                    return true;
                }
                return false;
            }
        }

        public string MetricsHistoryTimeString { 
            get
            {
                TimeSpan ts = TimeSpan.FromMinutes(MetricsHistoryMinutes);
                return string.Format($"{ts.Days}days {ts.Hours}hrs {ts.Minutes}mins");
            }
        }

        public ObservableCollection<MetricTimeSeriesData> PerformanceMetricSeries { get; set; } = new ObservableCollection<MetricTimeSeriesData>(); 

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RestSqlDb()
        {
            OverSpendFromMaxPcString = "?";
            PotentialSavingAmountString = "?";
        }

        public void CalcPotentialSaving()
        {
            PotentialSavingAmount = 0;
            if (TotalCostBilling <= 6) return; // too small to decrease

            if (IsElaticPoolMember)
            {
                if (properties.currentServiceObjectiveName == "ElasticPool GP_Gen5 2")
                {
                    if (OverSpendFromMaxPc > 75)
                    {
                        // cost reuction is approx $400 pm to $270 for DTU Standard 100DTUs - 33%ish
                        // calc % not hard code to cater for diff currencies
                        PotentialSavingAmount = TotalCostBilling * (decimal)0.33;

                        PotentialSavingDescription = $"Already cheapest VCore EPool. Change to DTU as usage very low ({100 - OverSpendFromMaxPc}%)";
                    }
                }
                else
                {

                    // not cheapest vcore but % of the cost is storage and licensing so approx 50% can be reduced
                    var nonStorageCost = TotalCostBilling * (decimal)0.5;
                    if (OverSpendFromMaxPc > 75)
                    {
                        PotentialSavingAmount = nonStorageCost * (decimal)0.75;
                        PotentialSavingDescription = $"Reducing VCore component cost of {nonStorageCost} (not license or storage) by 75% as max usage under 25%.";
                    }
                    if (OverSpendFromMaxPc > 50 && OverSpendFromMaxPc <= 75)
                    {
                        PotentialSavingAmount = nonStorageCost * (decimal)0.50;
                        PotentialSavingDescription = $"Reducing VCore component cost of {nonStorageCost} (not license or storage) by 50% as max usage between 25% and 50%.";
                    }
                }
            }
            else
            {
                // DTU is linear with no seperate storage/licensing costs
                if (OverSpendFromMaxPc > 75)
                {
                    PotentialSavingAmount = TotalCostBilling * (decimal)0.75;
                    PotentialSavingDescription = $"Reducing DTU cost by 75% as max usage under 25%.";
                }
                if (OverSpendFromMaxPc > 50 && OverSpendFromMaxPc <= 75)
                {
                    PotentialSavingAmount = TotalCostBilling * (decimal)0.50;
                    PotentialSavingDescription = $"Reducing DTU cost by 50% as max usage between 25% and 50%.";
                }
            }

        }

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
        public string elasticPoolId { get; set; }
        public string elasticPoolName { get; set; }
        public bool zoneRedundant { get; set; }
        public string currentBackupStorageRedundancy { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataEstateOverview.Models.Rest;

namespace DbMeta.Ui.Wpf.Models.Rest
{
    public class RootVM
    {
        public List<VM> value { get; set; } 
    }
    public class VM : INotifyPropertyChanged
    {
        public VMProperties properties { get; set; }
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string resourceGroup { get; set; }
        public string location { get; set; }
        public Subscription Subscription { get; set; }
        public decimal TotalCostBilling { get; set; }
        public List<ResourceCost> Costs { get; set; } = new List<ResourceCost>();

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
        public decimal _maxCpuUsed { get; set; }
        public decimal MaxCpuUsed
        {
            get { return _maxCpuUsed; }
            set
            {
                _maxCpuUsed = value;
                OnPropertyChanged("MaxCpuUsed");
            }
        }
        public ObservableCollection<MetricTimeSeriesData> PerformanceMetricSeries { get; set; } = new ObservableCollection<MetricTimeSeriesData>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class VMProperties
    {
        public string vmId { get; set; }
        public VMHardwareProfile hardwareProfile { get; set; }
        public VMOSProfile osProfile { get; set; }
        public DateTime timeCreated { get; set; }
    }
    public class VMHardwareProfile
    {
        public string vmSize { get; set; }
    }
    public class VMOSProfile
    {
        public string computerName { get; set; }
    }
}

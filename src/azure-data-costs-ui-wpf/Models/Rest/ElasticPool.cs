using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataEstateOverview.Models.Rest;

namespace Azure.Costs.Ui.Wpf.Models.Rest
{
    public class RootElasticPool
    {
        public ElasticPool[] value { get; set; }
    }

    public class ElasticPool : INotifyPropertyChanged
    {
        public string name { get; set; }
        public string kind { get; set; }

        public List<RestSqlDb> dbList { get; set; } = new List<RestSqlDb>();    

        public ElasticPoolProps properties { get; set; }
        public ElasticPoolSku sku { get; set; }

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
        public ObservableCollection<MetricTimeSeriesData> PerformanceMetricSeries { get; set; } = new ObservableCollection<MetricTimeSeriesData>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class ElasticPoolSku
    {
        public string name { get; set; }
        public string tier { get; set; }
        public string family { get; set; }
        public int capacity { get; set; }
    }
    public class ElasticPoolProps
    {
        public string state { get; set; }
        
        public long maxSizeBytes { get; set; }
        public DateTime creationDate { get; set; }
     
        public bool zoneRedundant { get; set; }
        
    }
}

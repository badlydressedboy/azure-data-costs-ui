using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{


    public class RootMetric
    {
        public int cost { get; set; }
        public Metric[] value { get; set; }
    }

    public class Metric
    {
        public string id { get; set; }
        public MetricName name { get; set; }
        public string unit { get; set; }
        public MetricTimeSeries[] timeseries { get; set; }

    }
    public class MetricName
    {
        public string value { get; set; }
    }
    public class MetricTimeSeries
    {
        public MetricTimeSeriesData[] data { get; set; }
    }
    public class MetricTimeSeriesData
    {
        public DateTime timeStamp { get; set; }
        public decimal average { get; set; }
        public decimal minimum { get; set; }
        public decimal maximum { get; set; }
    }
}

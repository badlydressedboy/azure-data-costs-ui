﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.Rest
{
    public class ResourceCostQuery
    {
        public ResourceCostProperties properties { get; set; }
    }

    public class ResourceCostProperties
    {
        public List<List<object>> rows { get; set; }
    }

    public class ResourceCost
    {
        public decimal Cost { get; set; }
        public decimal CostUSD { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceId { get; set; }
        public string ServiceName { get; set; }
        public string MeterCategory { get; set; }
        public string MeterSubCategory { get; set; }
        public string Product { get; set; }
        public string Meter { get; set; }
        public string ChargeType { get; set; }
        public string PublisherType { get; set; }
        public string Currency { get; set; }
        public string ResourceType { get; set; }
        public string ResourceName { get; set; }
    }

    public class ElasticPoolCost : ResourceCost
    {
        public decimal PerDbCost { get; set; }
        public int DbCount { get; set; }

        public ElasticPoolCost(ResourceCost cost)
        {
            Cost = cost.Cost;
            CostUSD = cost.CostUSD;
            SubscriptionId = cost.SubscriptionId;
            ResourceId = cost.ResourceId;
            ServiceName = cost.ServiceName;
            MeterCategory = cost.MeterCategory;
            MeterSubCategory = cost.MeterSubCategory;
            Product = cost.Product;
            Meter = cost.Meter;
            ChargeType = cost.ChargeType;
            PublisherType = cost.PublisherType;
            Currency = cost.Currency;
            ResourceType = cost.ResourceType;
            ResourceName = cost.ResourceName;
        }
    }
}

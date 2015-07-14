using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidClient
{
    public class UsageDetail
    {
        public string UsageDetailId { get; set; }
        public string SubscriptionId { get; set; }
        public string UsageStartTime { get; set; }
        public string UsageEndTime { get; set; }
        public string MeterName { get; set; }
        public string MeterRegion { get; set; }
        public string MeterCategory { get; set; }
        public string Unit { get; set; }
        public string MeterId { get; set; }
        public string Region { get; set; }
        public string Service { get; set; }
        public string ResourceName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal CostPerUnit { get; set; }
    }
}
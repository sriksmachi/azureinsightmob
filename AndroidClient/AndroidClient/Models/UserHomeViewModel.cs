using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidClient
{
    public class UserHomeViewModel
    {
        public List<SerializableResource> Resources { get; set; }
        public List<string> MeterCategories { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public List<Subscription> Subscriptions { get; set; }
        public List<UsageDetail> UsageDetails { get; set; }
        public List<UsageDetail> UsageByResource { get; set; }
        public string Currency { get; set; }
        public string AccessToken { get; set; }
    }
}
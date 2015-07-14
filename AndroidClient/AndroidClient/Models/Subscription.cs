using Java.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace AndroidClient
{
    public class Subscription 
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string OrganizationId { get; set; }
        public bool IsConnected { get; set; }
        public DateTime ConnectedOn { get; set; }
        public string ConnectedBy { get; set; }
        public UsagePayload usagePayload { get; set; }
        public string UsageString { get; set; }
        public bool AzureAccessNeedsToBeRepaired { get; set; }
    }

    public class SubscriptionInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string OrganizationId { get; set; }
        public string AccessToken { get; set; }
        public string Currency { get; set; }
    }
}
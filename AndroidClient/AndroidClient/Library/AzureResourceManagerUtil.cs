using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AndroidClient.Library
{
    public static class AzureResourceManagerUtil
    {
        public static string azureResourceManagerURL = "https://management.azure.com";
        public static string azureResourceManagerAPIVersion = "2014-04-01-preview";
        public static string clientId = "0618fcde-7618-409e-81fd-821af6d2d593";
        public static string armBillingServiceURL = "https://management.azure.com";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static UserHomeViewModel GetSubscriptions(string accessToken)
        {
            try
            {
                var organizations = GetUserOrganizations(accessToken);
                UserHomeViewModel vm = new UserHomeViewModel();
                foreach (Organization org in organizations)
                {
                    if (org.Id != null)
                    {
                        var subscriptions = GetUserSubscriptions(org.Id, accessToken);
                        vm.Subscriptions = subscriptions;
                    }
                }
                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static UserHomeViewModel GetUsageDetailsBySubscriptionId(string subscriptionId,
                string subName, string orgId, string accessToken, string currency)
        {
            try
            {
                UserHomeViewModel vm = null;
                List<UsageDetail> usageDetails = null;
                var usagePayLoad = GetUsage(subscriptionId, orgId, accessToken);
                if (usagePayLoad != null && usagePayLoad.value != null && usagePayLoad.value.Count > 0)
                {
                    vm = new UserHomeViewModel();
                    vm.SubscriptionId = subscriptionId;
                    vm.SubscriptionName = subName;
                    vm.Currency = currency;
                    var rateCard = AzureResourceManagerUtil.GetRatesForSubscription(subscriptionId, accessToken, vm.Currency);
                    if (usagePayLoad != null && usagePayLoad.value != null && usagePayLoad.value.Any())
                    {
                        usageDetails = new List<UsageDetail>();
                        foreach (var usageAggregate in usagePayLoad.value)
                        {
                            var usageDetail = new UsageDetail();
                            usageDetail.UsageDetailId = usageAggregate.id;
                            usageDetail.SubscriptionId = usageAggregate.properties.subscriptionId;
                            usageDetail.UsageStartTime = usageAggregate.properties.usageStartTime;
                            usageDetail.UsageEndTime = usageAggregate.properties.usageEndTime;
                            usageDetail.MeterId = usageAggregate.properties.meterId;
                            usageDetail.MeterName = usageAggregate.properties.meterName;
                            usageDetail.MeterRegion = usageAggregate.properties.meterRegion;
                            usageDetail.MeterCategory = usageAggregate.properties.meterCategory;
                            usageDetail.Unit = usageAggregate.properties.unit;
                            usageDetail.Region = usageAggregate.properties.infoFields.meteredRegion;
                            usageDetail.Service = usageAggregate.properties.infoFields.meteredService;
                            usageDetail.ResourceName = usageAggregate.properties.infoFields.project;
                            usageDetail.Quantity = (decimal)usageAggregate.properties.quantity;
                            var meter = rateCard.Meters.FirstOrDefault(m => m.MeterId == usageDetail.MeterId);
                            if (meter != null && meter.MeterRates[0] != 0)
                            {
                                usageDetail.Cost = RoundTo2Digits(usageDetail.Quantity * meter.MeterRates[0]);
                                if (usageDetail.Cost > 0)
                                {
                                    usageDetail.CostPerUnit = meter.MeterRates[0];
                                    usageDetails.Add(usageDetail);
                                }
                            }
                        }
                        vm.UsageDetails = usageDetails;
                    }
                    vm.UsageByResource = usageDetails.GroupBy(x => x.MeterCategory)
                              .Select(g => new UsageDetail { MeterId = g.Key, Cost = g.Sum(x => x.Cost) })
                              .OrderByDescending(m => m.Cost).ToList();
                }
                return vm;
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public static RateCardPayload GetRatesForSubscription(string subscriptionId, string accessToken, string currency)
        {
            try
            {
                RateCardPayload payload = null;
                string local = string.Empty;
                var key = "ratecard-" + currency;
                var ratecardCache = LocalStoreManager.SQliteConnection.Table<KeyValueModel>()
                                    .Where(k => k.Key == key).FirstOrDefault();
                if (ratecardCache != null)
                {
                    payload = JsonConvert.DeserializeObject<RateCardPayload>(ratecardCache.Value);
                    return payload;
                }
                else
                {
                    if (currency == "INR")
                    {
                        local = string.Format("Currency eq 'INR' and Locale eq 'en-IN' and RegionInfo eq 'IN'");
                    }
                    else if (currency == "$")
                    {
                        local = string.Format("Currency eq 'USD' and Locale eq 'en-US' and RegionInfo eq 'US'");
                    }

                    string requestURL = String.Format("{0}/{1}/{2}/{3}",
                           armBillingServiceURL,
                           "subscriptions",
                           subscriptionId,
                           "providers/Microsoft.Commerce/RateCard?api-version=2015-06-01-preview&$filter=OfferDurableId eq 'MS-AZR-0121p' and " + local);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);
                    // Add the OAuth Authorization header, and Content Type header
                    request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
                    request.ContentType = "application/json";
                    // Call the RateCard API, dump the output to the console window
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream receiveStream = response.GetResponseStream();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        var rateCardResponse = readStream.ReadToEnd();
                        payload = JsonConvert.DeserializeObject<RateCardPayload>(rateCardResponse);
                        LocalStoreManager.SQliteConnection.Insert(new KeyValueModel()
                        {
                            Key = key,
                            Value = JsonConvert.SerializeObject(payload)
                        });
                        response.Close();
                        readStream.Close();
                    }
                    catch { throw; }
                    return payload;
                }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static List<Organization> GetUserOrganizations(string accessToken)
        {
            List<Organization> organizations = new List<Organization>();
            try
            {
                // Get a list of Organizations of which the user is a member            
                string requestUrl = string.Format("{0}/tenants?api-version={1}", azureResourceManagerURL,
                    azureResourceManagerAPIVersion);
                // Make the GET request
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var organizationsJSONArray = JObject.Parse(responseContent);
                    foreach (JObject organization in organizationsJSONArray["value"])
                    {
                        organizations.Add(new Organization()
                           {
                               Id = (String)organization["tenantId"],
                           });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return organizations;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static List<Subscription> GetUserSubscriptions(string organizationId, string accessToken)
        {
            List<Subscription> subscriptions = null;
            try
            {
                // Aquire Access Token to call Azure Resource Manager
                subscriptions = new List<Subscription>();
                // Get subscriptions to which the user has some kind of access
                string requestUrl = string.Format("{0}/subscriptions?api-version={1}",
                    azureResourceManagerURL,
                    azureResourceManagerAPIVersion);
                // Make the GET request
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var subscriptionsJSONArray = JObject.Parse(responseContent);
                    foreach (JObject subs in subscriptionsJSONArray["value"])
                    {
                        subscriptions.Add(new Subscription()
                         {
                             Id = (String)subs["subscriptionId"],
                             DisplayName = (String)subs["displayName"],
                             OrganizationId = organizationId
                         });
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return subscriptions;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static UsagePayload GetUsage(string subscriptionId, string organizationId, string accessToken)
        {
            UsagePayload usagePayLoad = new UsagePayload();
            usagePayLoad.value = new List<UsageAggregate>();
            string UsageResponse = "";
            try
            {
                Dictionary<string, string> startAndEndDates = new Dictionary<string, string>();
                for (int i = 0; i > -3 ; i--)
                {
                    var lastDayInMonth = i == 0 ? DateTime.Now.Day : DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.AddMonths(i).Month);
                    string endDate = DateTime.UtcNow.AddMonths(i).ToString(string.Format("yyyy-MM-{0}", lastDayInMonth));
                    string startDate = DateTime.UtcNow.AddMonths(i).ToString("yyyy-MM-01");
                    startAndEndDates.Add(startDate, endDate);
                }
                foreach (var key in startAndEndDates.Keys)
                {
                    // Aquire Access Token to call Azure Resource Manager
                    string requesturl = String.Format("https://management.azure.com/subscriptions/{0}/providers/Microsoft.Commerce/UsageAggregates?api-version=2015-06-01-preview&reportedstartTime={1}+00%3a00%3a00Z&reportedEndTime={2}+00%3a00%3a00Z",
                        subscriptionId, key, startAndEndDates[key]);
                    //Crafting the HTTP call
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requesturl);
                    request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
                    request.ContentType = "application/json";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    UsageResponse = readStream.ReadToEnd();
                    var usagePayLoadResponse = JsonConvert.DeserializeObject<UsagePayload>(UsageResponse);
                    usagePayLoad.value.AddRange(usagePayLoadResponse.value);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return usagePayLoad;
        }

        /// <summary>
        /// Rounds decimal to 4 digits after decimal
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static decimal RoundTo2Digits(decimal input)
        {
            return decimal.Round(input, 2);
        }

        public static string ToCurrency(this decimal input, string currency)
        {
            try
            {
                var actualCurrency = decimal.Round(input, 2).ToString("C");
                var subStr = actualCurrency.Substring(1, actualCurrency.Length - 1);
                return currency + " " + subStr;
            }
            catch (Exception ex) { throw ex; }
        }
    }
}
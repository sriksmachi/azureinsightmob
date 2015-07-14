using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AndroidClient.Library;
using System.Threading.Tasks;

namespace AndroidClient
{
    [Activity(Label = "Subscription Usage : Last 3 Months", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class SubscriptionDetails : Activity
    {
        public Handler handler = null;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.SubscriptionDetails);
                handler = new Handler(BackgroundThreadMessageHandler);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        private void BackgroundThreadMessageHandler(Message m)
        {
            string message = string.Empty;
            switch (m.What)
            {
                case -1:
                    message = "No data available for this subscription, please try again later.";
                    break;
                case 404:
                    var details = m.Data.GetString("error");
                    message = "There is some problem with the billing APIs, please try again later. \nDetails:" + details;
                    break;
                case 1:
                    var progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                    if (progressBar != null)
                    {
                        progressBar.Visibility = ViewStates.Gone;
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(message))
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Information");
                builder.SetMessage(message);
                builder.Create().Show();
            }
        }

        protected override void OnStart()
        {
            try
            {
                var selectedSubscription = Intent.GetStringExtra("selectedsubscription");
                var subId = JObject.Parse(selectedSubscription)["Id"].Value<string>();
                var subName = JObject.Parse(selectedSubscription)["DisplayName"].Value<string>();
                var orgId = JObject.Parse(selectedSubscription)["OrganizationId"].Value<string>();
                var accessToken = JObject.Parse(selectedSubscription)["AccessToken"].Value<string>();
                var currency = JObject.Parse(selectedSubscription)["Currency"].Value<string>();
                Task.Factory.StartNew(() =>
                 {
                     try
                     {
                         var userHomeViewModel = GetUsageDetails(subId, subName, orgId, accessToken, currency);
                         handler.SendMessage(new Message() { What = 1 });
                         if (userHomeViewModel != null)
                         {
                             BindEvents(userHomeViewModel);
                             BindDefault(userHomeViewModel);
                         }
                         else
                         {
                             handler.SendMessage(new Message() { What = -1 });
                         }
                     }
                     catch (Exception ex)
                     {
                         Bundle errorData = new Bundle();
                         errorData.PutString("error", ex.Message);
                         handler.SendMessage(new Message() { What = 404, Data = errorData });
                         handler.SendMessage(new Message() { What = 1 });
                     }
                 });
                base.OnStart();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }


        private void BindDefault(UserHomeViewModel userHomeViewModel)
        {
            try
            {
                var fragmentDefault = new SubscriptionDetailsFragment();
                fragmentDefault.LayoutId = Resource.Layout.FrameLayoutDefault;
                fragmentDefault.UserHomeViewModel = userHomeViewModel;
                FragmentManager.BeginTransaction().Add(Resource.Id.framecontent, fragmentDefault).Commit();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usageDetails"></param>
        private void BindEvents(UserHomeViewModel usageDetails)
        {
            var btnOne = this.FindViewById<Button>(Resource.Id.buttonTwo);
            btnOne.Click += (object sender, EventArgs e) =>
            {
                var fragment = new ResourcesFragment();
                fragment.LayoutId = Resource.Layout.FrameLayoutOne;
                fragment.UserHomeViewModel = usageDetails;
                var transaction = FragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.framecontent, fragment).Commit();
            };
            var btnDefault = this.FindViewById<Button>(Resource.Id.buttonDefault);
            btnDefault.Click += (object sender, EventArgs e) =>
            {
                BindDefault(usageDetails);
            };
            var btnThree = this.FindViewById<Button>(Resource.Id.buttonThree);
            btnThree.Click += (object sender, EventArgs e) =>
            {
                var fragment = new DailyFragment();
                fragment.LayoutId = Resource.Layout.FrameLayoutThree;
                fragment.UserHomeViewModel = usageDetails;
                FragmentManager.BeginTransaction().Replace(Resource.Id.framecontent, fragment).Commit();
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subId"></param>
        /// <param name="subName"></param>
        /// <param name="orgId"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private static UserHomeViewModel GetUsageDetails(string subId, string subName, string orgId, string accessToken, string currency)
        {
            var key = string.Format("{0}-{1}-{2}", subName, DateTime.Now.ToShortDateString(), currency);
            var userDataModel = LocalStoreManager.SQliteConnection.Table<KeyValueModel>().Where(k => k.Key == key).FirstOrDefault();
            if (userDataModel == null)
            {
                var usageDetails = AzureResourceManagerUtil.GetUsageDetailsBySubscriptionId(subId, subName, orgId, accessToken, currency);
                if (usageDetails != null)
                {
                    LocalStoreManager.SQliteConnection.Insert(new KeyValueModel()
                    {
                        Key = key,
                        Value = JsonConvert.SerializeObject(usageDetails)
                    });
                }
                return usageDetails;
            }
            else
            {
                var cachedValue = JsonConvert.DeserializeObject<UserHomeViewModel>(userDataModel.Value);
                cachedValue.Currency = currency;
                return cachedValue;
            }
        }
    }
}
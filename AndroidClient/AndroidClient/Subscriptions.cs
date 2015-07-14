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
using AndroidClient.Library;
using AndroidClient.Models;

namespace AndroidClient
{
    [Activity(Label = "Subscriptions")]
    public class Subscriptions : Activity
    {
        UserHomeViewModel userViewModel;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                SetContentView(Resource.Layout.Subscriptions);
                var userHomeViewModel = Intent.GetStringExtra("userhomeviewmodel");
                userViewModel = JsonConvert.DeserializeObject<UserHomeViewModel>(userHomeViewModel);
                var userLoginContextStore = LocalStoreManager.SQliteConnection.Table<KeyValueModel>().Where(k => k.Key == "userlogincontext").FirstOrDefault();
                if (userLoginContextStore != null)
                {
                    var userLoginContextObj = JsonConvert.DeserializeObject<UserLoginContext>(userLoginContextStore.Value);
                    FindViewById<TextView>(Resource.Id.txtloginas).Text = string.Format("Welcome {0}", userLoginContextObj.Username);
                }
                var gridView = FindViewById<GridView>(Resource.Id.gvCtrl);
                gridView.ItemClick += gridView_ItemClick;
                gridView.Adapter = new GridViewAdapter(this, userViewModel);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void gridView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                var subscription = userViewModel.Subscriptions[e.Position];
                var subscriptionInfo = new SubscriptionInfo()
                {
                    Id = subscription.Id,
                    DisplayName = subscription.DisplayName,
                    OrganizationId = subscription.OrganizationId,
                    AccessToken = userViewModel.AccessToken,
                    Currency = userViewModel.Currency
                };
                var intent = new Intent(this, typeof(SubscriptionDetails));
                var subscriptionJSON = JsonConvert.SerializeObject(subscriptionInfo);
                intent.PutExtra("selectedsubscription", subscriptionJSON);
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }
    }
}
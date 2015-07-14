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
using AndroidClient.Library;

namespace AndroidClient
{
    public class SubscriptionDetailsFragment : Fragment
    {
        public int LayoutId;

        public UserHomeViewModel UserHomeViewModel { get; set; }

        public SubscriptionDetailsFragment() { }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                if (LayoutId > 0)
                {
                    var view = inflater.Inflate(LayoutId, container, false);
                    BindUI(view);
                    return view;
                } return null;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this.Activity);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usageDetails"></param>
        private void BindUI(View view)
        {
            var totalCost = UserHomeViewModel.UsageByResource.Sum(u => u.Cost);
            var usageByDate = (from usageDetail in UserHomeViewModel.UsageDetails
                               orderby usageDetail.UsageEndTime descending
                               group usageDetail by usageDetail.UsageEndTime into grps
                               select new { DateTime = grps.Key, Values = grps, Cost = grps.Sum(g => g.Cost) }).ToList().Take(5);
            var top5Resources = (from usageDetail in UserHomeViewModel.UsageDetails
                                 orderby usageDetail.Cost descending
                                 group usageDetail by usageDetail.ResourceName into grps
                                 select new UsageDetail { ResourceName = grps.Key, Cost = grps.Sum(g => g.Cost) }).Take(5).ToList();
            var region =        (from usageDetail in UserHomeViewModel.UsageDetails
                                 orderby usageDetail.Cost descending
                                 group usageDetail by usageDetail.Region into grps
                                 select new UsageDetail { Region = grps.Key, Cost = grps.Sum(g => g.Cost) }).ToList();
            var textSubName = view.FindViewById<TextView>(Resource.Id.txtselectedsubname);
            if (textSubName != null)
            {
                textSubName.Text = UserHomeViewModel.SubscriptionName;
            }
            var textTotalCost = view.FindViewById<TextView>(Resource.Id.txtselectedsubcost);
            if (textTotalCost != null)
            {
                textTotalCost.Text = totalCost.ToCurrency(UserHomeViewModel.Currency);
            }
            var textAvg = view.FindViewById<TextView>(Resource.Id.txtselectedsubavg);
            if (textAvg != null)
            {
                textAvg.Text = ((usageByDate.Sum(a => a.Cost)) / 5).ToCurrency(UserHomeViewModel.Currency);
            }
            var top5GridView = view.FindViewById<GridView>(Resource.Id.top5GVCtrl);
            top5GridView.Adapter = new Top5GridViewAdapter(this.Activity, top5Resources, UserHomeViewModel.Currency);
            var regioGridView = view.FindViewById<GridView>(Resource.Id.regionGVCtrl);
            regioGridView.Adapter = new RegionGridViewAdapter(this.Activity, region, UserHomeViewModel.Currency);
        }
    }
}
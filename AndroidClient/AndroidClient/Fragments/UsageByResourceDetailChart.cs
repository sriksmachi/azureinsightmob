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
using Newtonsoft.Json.Linq;
using BarChart;
using AndroidClient.Library;

namespace AndroidClient
{
    [Activity(Label = "Usage By Resources")]
    public class UsageByResourceDetailChart : Fragment
    {
        public int LayoutId;
        public UsageDetail SelectedResource;
        public UserHomeViewModel UserHomeViewModel;

        public UsageByResourceDetailChart()
        {
        }

        public override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                if (LayoutId == 0) return null;
                var view = inflater.Inflate(LayoutId, container, false);
                BindUI(view);
                return view;
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
        /// <param name="view"></param>
        private void BindUI(Android.Views.View view)
        {
            try
            {
                var chart = view.FindViewById<BarChartView>(Resource.Id.barChart1);
                var usageByResource = (from userDetail in UserHomeViewModel.UsageDetails
                                       where userDetail.MeterCategory == SelectedResource.MeterId
                                       orderby userDetail.Cost descending
                                       group userDetail by userDetail.ResourceName into grps
                                       select new BarModel
                                       {
                                           Legend = grps.Key,
                                           Value = (float)grps.Sum(c => c.Cost),
                                           //ValueCaptionHidden = true,
                                           Color = Android.Graphics.Color.White
                                       }).ToList();
                view.FindViewById<TextView>(Resource.Id.selectedMeterCategory).Text = SelectedResource.MeterId;
                chart.MinimumValue = 0;
                chart.BarWidth = 120;
                chart.BarClick += chart_BarClick;
                chart.LegendHidden = true;
                chart.ItemsSource = usageByResource;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this.Activity);
            }
        }

        void chart_BarClick(object sender, BarClickEventArgs e)
        {
            try
            {
                BarModel barModel = e.Bar;
                this.Activity.FindViewById<TextView>(Resource.Id.selectedBar).Text = string.Format("Resource Name: {0}, Cost: {1}", barModel.Legend, barModel.Value);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this.Activity);
            }
        }

    }
}
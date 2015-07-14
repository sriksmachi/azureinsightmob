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
using AndroidClient.Models;
using System.Globalization;
using AndroidClient.Library;

namespace AndroidClient
{
    [Activity(Label = "Usage By Day")]
    public class UsageByDayDetailChart : Fragment
    {
        public int LayoutId;
        public UsageByMonth SelectedMonth;
        public UserHomeViewModel UserHomeViewModel;

        public UsageByDayDetailChart() 
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
            var chart = view.FindViewById<BarChartView>(Resource.Id.dailyBarChart);
            var dailyUsageDetails = new List<BarModel>();
            foreach(var usageDetail in UserHomeViewModel.UsageDetails)
            {
                var usageDate = DateTime.Parse(usageDetail.UsageEndTime);
                var key = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(usageDate.Month) + "-" + usageDate.Year;
                if(key == SelectedMonth.Month)
                {
                    if (dailyUsageDetails.Any(d => d.Legend == usageDate.ToShortDateString()))
                    {
                        var existingDailyUsage = dailyUsageDetails.FirstOrDefault(d => d.Legend == usageDate.ToShortDateString());
                        existingDailyUsage.Value += (float)usageDetail.Cost;
                    }
                    else
                    {
                        dailyUsageDetails.Add(new BarModel()
                        {
                            Legend = usageDate.ToShortDateString(),
                            Value = (float)usageDetail.Cost,
                            Color = Android.Graphics.Color.White
                            //ValueCaptionHidden = true
                        });
                    }
                }
            }
            view.FindViewById<TextView>(Resource.Id.selectedMonth).Text = SelectedMonth.Month;
            chart.MinimumValue = 0;
            chart.BarWidth = 120;
            chart.BarClick += chart_BarClick;
            chart.LegendHidden = true;
            chart.ItemsSource = dailyUsageDetails;
        }

        void chart_BarClick(object sender, BarClickEventArgs e)
        {
            try
            {
                BarModel barModel = e.Bar;
                this.Activity.FindViewById<TextView>(Resource.Id.selectedBarDay).Text = string.Format("Date: {0}, Cost: {1} {2}", barModel.Legend,
                    UserHomeViewModel.Currency,
                    barModel.Value);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this.Activity);
            }
        }
       
    }
}
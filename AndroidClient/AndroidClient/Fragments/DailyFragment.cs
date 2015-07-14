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
using AndroidClient.Models;
using System.Globalization;
using AndroidClient.Library;

namespace AndroidClient
{
    public class DailyFragment : Fragment
    {
        public int LayoutId;
        public UserHomeViewModel UserHomeViewModel { get; set; }
        private List<UsageByMonth> monthlyUsage { get; set; }

        public DailyFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (LayoutId == 0) return null;
            var view = inflater.Inflate(LayoutId, container, false);
            BindUI(view);
            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usageDetails"></param>
        private void BindUI(View view)
        {
            try
            {
                var gridView = view.FindViewById<GridView>(Resource.Id.monthlyGVCtrl);
                monthlyUsage = new List<UsageByMonth>();
                foreach (var usageDetail in UserHomeViewModel.UsageDetails)
                {
                    var usageByMonth = new UsageByMonth();
                    var usageDate = DateTime.Parse(usageDetail.UsageEndTime);
                    var key = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(usageDate.Month) + "-" + usageDate.Year;
                    if (monthlyUsage.Any(m => m.Month == key))
                    {
                        var existingRecord = monthlyUsage.FirstOrDefault(m => m.Month == key);
                        existingRecord.Cost += usageDetail.Cost;
                    }
                    else
                    {
                        usageByMonth.Month = key;
                        usageByMonth.Cost = usageDetail.Cost;
                        monthlyUsage.Add(usageByMonth);
                    }
                }
                gridView.ItemClick += gridView_ItemClick;
                gridView.Adapter = new MonthlyGridViewAdapter(this.Activity, monthlyUsage, UserHomeViewModel.Currency);
            }
            catch (Exception ex) {
                ExceptionHandler.HandleException(ex, this.Activity);
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
                var selectedUsageByMonth = monthlyUsage[e.Position];
                var fragmentDefault = new UsageByDayDetailChart();
                fragmentDefault.LayoutId = Resource.Layout.UsageByDayChart;
                fragmentDefault.SelectedMonth = selectedUsageByMonth;
                fragmentDefault.UserHomeViewModel = UserHomeViewModel;
                var fragmentTransaction = FragmentManager.BeginTransaction();
                fragmentTransaction.Replace(Resource.Id.framecontent, fragmentDefault);
                fragmentTransaction.AddToBackStack("UsageByDayDetailChart");
                fragmentTransaction.Commit();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this.Activity);
            }
        }
    }
}
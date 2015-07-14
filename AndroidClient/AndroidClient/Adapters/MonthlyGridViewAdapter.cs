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
using System.Globalization;
using AndroidClient.Library;
using AndroidClient.Models;

namespace AndroidClient
{
    public class MonthlyGridViewAdapter : BaseAdapter
    {
        private List<UsageByMonth> _usageByMonthDetails;
        private readonly Activity _activity;
        private string _currency;

        public MonthlyGridViewAdapter(Activity activity, List<UsageByMonth> usageDetails, string currency)
        {
            _activity = activity;
            _usageByMonthDetails = usageDetails;
            _currency = currency;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="convertView"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                var item = _usageByMonthDetails[position];
                if (convertView == null)
                    convertView = _activity.LayoutInflater.Inflate(Resource.Layout.monthlyGridView, null);
                convertView.FindViewById<TextView>(Resource.Id.txtMonthName).Text = item.Month;
                convertView.FindViewById<TextView>(Resource.Id.txtMonthCost).Text = item.Cost.ToCurrency(_currency);
                return convertView;
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, _activity);
            }
            return null;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get
            {
                return _usageByMonthDetails == null ? -1 : _usageByMonthDetails.Count;
            }
        }
    }
}
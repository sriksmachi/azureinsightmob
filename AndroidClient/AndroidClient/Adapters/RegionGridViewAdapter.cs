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
    public class RegionGridViewAdapter : BaseAdapter
    {
        private List<UsageDetail> _usageDetails;
        private readonly Activity _activity;
        private string _currency;

        public RegionGridViewAdapter(Activity activity, List<UsageDetail> usageDetails, string currency)
        {
            _activity = activity;
            _usageDetails = usageDetails;
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
                var item = _usageDetails[position];
                if (convertView == null)
                    convertView = _activity.LayoutInflater.Inflate(Resource.Layout.regionGridView, null);
                convertView.FindViewById<TextView>(Resource.Id.txtRegionName).Text = item.Region ?? "Unknown";
                convertView.FindViewById<TextView>(Resource.Id.txtRegionCost).Text = item.Cost.ToCurrency(_currency);
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
                return _usageDetails == null ? -1 : _usageDetails.Count;
            }
        }
    }
}
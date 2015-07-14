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
    public class GridViewAdapter : BaseAdapter
    {
        private UserHomeViewModel _usageHomeViewModel;
        private readonly Activity _activity;

        public GridViewAdapter(Activity activity, UserHomeViewModel userHomeViewModel)
        {
            _activity = activity;
            _usageHomeViewModel = userHomeViewModel;
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
                var item = _usageHomeViewModel.Subscriptions[position];
                if (convertView == null)
                    convertView = _activity.LayoutInflater.Inflate(Resource.Layout.custGridViewItem, null);
                convertView.FindViewById<TextView>(Resource.Id.txtSubName).Text = item.DisplayName;
                convertView.FindViewById<TextView>(Resource.Id.txtSubId).Text = item.Id;
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
                return _usageHomeViewModel.Subscriptions == null ? -1 : _usageHomeViewModel.Subscriptions.Count;
            }
        }
    }
}
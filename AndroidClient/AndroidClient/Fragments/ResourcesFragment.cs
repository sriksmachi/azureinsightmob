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

namespace AndroidClient
{
    public class ResourcesFragment : Fragment
    {
        public int LayoutId;

        public UserHomeViewModel UserHomeViewModel { get; set; }

        public ResourcesFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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
        /// <param name="usageDetails"></param>
        private void BindUI(View view)
        {
            var gridView = view.FindViewById<GridView>(Resource.Id.resourcesGVCtrl);
            gridView.ItemClick += gridView_Click;
            gridView.Adapter = new ResourcesGridViewAdapter(this.Activity, UserHomeViewModel.UsageByResource, UserHomeViewModel.Currency);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void gridView_Click(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                var usageByResource = UserHomeViewModel.UsageByResource[e.Position];
                var fragmentDefault = new UsageByResourceDetailChart();
                fragmentDefault.LayoutId = Resource.Layout.UsageByResourceChart;
                fragmentDefault.SelectedResource = usageByResource;
                fragmentDefault.UserHomeViewModel = UserHomeViewModel;
                var fragmentTransaction = FragmentManager.BeginTransaction();
                fragmentTransaction.Replace(Resource.Id.framecontent, fragmentDefault);
                fragmentTransaction.AddToBackStack("UsageByResourceDetailChart");
                fragmentTransaction.Commit();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this.Activity);
            }
        }
    }
}
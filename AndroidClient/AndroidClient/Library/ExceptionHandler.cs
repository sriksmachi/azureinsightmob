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

namespace AndroidClient.Library
{
    public static class ExceptionHandler
    {
        public static void HandleException(Exception ex, Activity activity)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetTitle("Error");
            builder.SetMessage(ex.Message);
            builder.Create().Show();
        }

        public static void HandleException(AggregateException ex, Activity activity)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetTitle("One or more Error(s)");
            builder.SetMessage("First:" + ex.InnerExceptions.First().Message);
            builder.Create().Show();
        }
    }
}
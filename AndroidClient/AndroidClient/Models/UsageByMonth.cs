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

namespace AndroidClient.Models
{
    public class UsageByMonth
    {
        public string Month { get; set; }
        public decimal Cost { get; set; }
    }
}
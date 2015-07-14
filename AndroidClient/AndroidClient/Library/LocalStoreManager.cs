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
using SQLite;
using System.IO;

namespace AndroidClient
{
    public class LocalStoreManager
    {
        static SQLiteConnection _connection;

        public LocalStoreManager() { }

        public static SQLiteConnection SQliteConnection
        {
            get
            {
                if (_connection == null)
                {
                    string dbPath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
        "azureinsight.db3");
                    _connection = new SQLite.SQLiteConnection(dbPath);
                }
                return _connection;
            }
        }
    }

    [Table("KeyValues")]
    public class KeyValueModel
    {
        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
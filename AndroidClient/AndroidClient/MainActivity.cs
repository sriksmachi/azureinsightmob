using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
//using Com.Microsoft.Adal;
using AndroidClient.Library;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using AndroidClient.Models;

namespace AndroidClient
{
    [Activity(Label = "AzureInsight", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        internal static readonly string AadAuthority = "https://login.windows.net/common";
        internal static readonly string AadClientID = "dfe139a4-3ede-4548-8ce9-a6747379c647";
        internal static readonly string AadRedirect = "http://myapp";
        internal static readonly string AadResource = "https://management.core.windows.net/";
        public UserHomeViewModel userViewModel = null;
        public Handler handler = null;
        string username = null;
        string password = null;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.Main);
                var tableInfo = LocalStoreManager.SQliteConnection.GetTableInfo("KeyValueModel");
                if (tableInfo.Count == 0)
                {
                    LocalStoreManager.SQliteConnection.CreateTable<KeyValueModel>();
                }
                LoadLoginContext();
                var loginBtn = FindViewById<Button>(Resource.Id.loginBtn);
                loginBtn.Click += loginBtn_Click;
                handler = new Handler(BackgroundThreadMessageHandler);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadLoginContext()
        {
            var userLoginContextStore = LocalStoreManager.SQliteConnection.Table<KeyValueModel>().Where(k => k.Key == "userlogincontext").FirstOrDefault();
            if (userLoginContextStore != null)
            {
                var userLoginContextObj = JsonConvert.DeserializeObject<UserLoginContext>(userLoginContextStore.Value);
                FindViewById<EditText>(Resource.Id.usernameET).Text = userLoginContextObj.Username;
                FindViewById<EditText>(Resource.Id.passwordET).Text = userLoginContextObj.Password;
                if (userLoginContextObj.Currency == "$")
                {
                    FindViewById<RadioButton>(Resource.Id.dollar).Checked = true;
                }
                else
                {
                    FindViewById<RadioButton>(Resource.Id.inr).Checked = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        private void BackgroundThreadMessageHandler(Message m)
        {
           string message = string.Empty;
           switch(m.What)
           {
               case 404:
                   var details = m.Data.GetString("error");
                   message = "Authentication Failed, make sure the credentials you are using are correct and linked to Azure AD.\nDetails: " + details;
                   break;
               default:
                   message = "Unknown error";
                   break;
           }
           AlertDialog.Builder builder = new AlertDialog.Builder(this);
           builder.SetTitle("Error");
           builder.SetMessage(message);
           builder.Create().Show();
           var loginBtn = FindViewById<Button>(Resource.Id.loginBtn);
           loginBtn.Text = "Login";
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnResume()
        {
            try
            {
                var loginBtn = FindViewById<Button>(Resource.Id.loginBtn);
                loginBtn.Text = "Login";
                base.OnResume();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void loginBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var loginBtn = FindViewById<Button>(Resource.Id.loginBtn);
                loginBtn.Text = "Signing In...";
                GetAccessToken(AadResource, this);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceResourceId"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async void GetAccessToken(string serviceResourceId, Activity activity)
        {
            try
            {
                Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext context =
                       new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(AadAuthority);
                if (context.TokenCache.ReadItems().Count() > 0)
                    context = new AuthenticationContext(context.TokenCache.ReadItems().First().Authority);
                username = FindViewById<EditText>(Resource.Id.usernameET).Text;
                password = FindViewById<EditText>(Resource.Id.passwordET).Text; ;
                UserCredential us = new UserCredential(username, password);
                await context.AcquireTokenAsync(serviceResourceId, AadClientID, us).ContinueWith((r) =>
                    {
                        OnSuccess(r);
                    }
                );
            }
            catch (AggregateException ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authResult"></param>
        public void OnSuccess(Task<AuthenticationResult> taskAuthResult)
        {
            try
            {
                var authResult = taskAuthResult.IsCompleted && !taskAuthResult.IsFaulted ? taskAuthResult.Result : null;
                if (authResult != null)
                {
                    var accessToken = authResult.AccessToken;
                    string username = authResult.UserInfo.DisplayableId;
                    var currency = FindViewById<RadioButton>(Resource.Id.dollar).Checked ? "$" : "INR";
                    var userLoginContext = new UserLoginContext()
                    {
                        Username = username,
                        Password = password,
                        Currency = currency
                    };
                    
                    //Store login context
                    var userLoginContextStore = LocalStoreManager.SQliteConnection.Table<KeyValueModel>().Where(k => k.Key == "userlogincontext").FirstOrDefault();
                    if(userLoginContextStore == null)
                    {
                        LocalStoreManager.SQliteConnection.Insert(new KeyValueModel()
                        {
                            Key = "userlogincontext",
                            Value = JsonConvert.SerializeObject(userLoginContext)
                        });
                    }

                    var userDataModel = LocalStoreManager.SQliteConnection.Table<KeyValueModel>().Where(k => k.Key == username).FirstOrDefault();
                    if (userDataModel == null)
                    {
                        //store data model
                        var userViewModel = AzureResourceManagerUtil.GetSubscriptions(accessToken);
                        userViewModel.Currency = currency;
                        userViewModel.AccessToken = accessToken;
                        LocalStoreManager.SQliteConnection.Insert(new KeyValueModel()
                        {
                            Key = username,
                            Value = JsonConvert.SerializeObject(userViewModel)
                        });
                        RenderView(userViewModel);
                    }
                    else
                    {
                        var cachedValue = JsonConvert.DeserializeObject<UserHomeViewModel>(userDataModel.Value);
                        cachedValue.Currency = currency;
                        RenderView(cachedValue);
                    }
                }
                else
                {
                    Bundle errorData = new Bundle();
                    errorData.PutString("error", taskAuthResult.Exception.InnerExceptions.FirstOrDefault().Message);
                    handler.SendMessage(new Message() { What = 404, Data =  errorData});
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        public void RenderView(UserHomeViewModel vm)
        {
            try
            {
                var intent = new Intent(this, typeof(Subscriptions));
                var userhomeViewModel = JsonConvert.SerializeObject(vm);
                intent.PutExtra("userhomeviewmodel", userhomeViewModel);
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, this);
            }
        }
    }
}


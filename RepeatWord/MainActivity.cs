using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Content;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System.IO;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;

namespace RepeatWord
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            AskPermissions();
            WordsManager.Instance.Init();

            Button button = FindViewById<Button>(Resource.Id.btnGoRepeatWords);
            button.Click += (sender, e) =>
            {
                 var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("IsLearn", false);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnGoLearnForgottenWords);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("IsLearn", true);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnSettings);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(SettingActivity));
                StartActivity(intent);
            };
        }
        
        void AskPermissions()
        {
            if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
            {
                var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                RequestPermissions(permissions, 1);
            }
        }
    }
}
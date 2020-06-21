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
using System.IO;
using Android;
using Android.Content.PM;
using static Android.App.ActionBar;

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
            WordsManager.Instance.Init(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "RepeatWord"));

            Button button = FindViewById<Button>(Resource.Id.btnLearnActiveWords);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(LearnActiveWordsActivity));
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnDailyRepeat);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("Type", (int)RepeatSessionType.DAILY_REPEAT);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnDailyRepeatRandom);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("Type", (int)RepeatSessionType.DAILY_REPEAT_RANDOM);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnGoRepeatWords);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("Type", (int)RepeatSessionType.FULL_REPEAT);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnGoRepeatWordsReversed);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("Type", (int)RepeatSessionType.FULL_REPEAT_REVERSED);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnGoLearnForgottenWords);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(RepeatWordActivity));
                intent.PutExtra("Type", (int)RepeatSessionType.REPEAT_FORGOTTEN);
                StartActivity(intent);
            };

            button = FindViewById<Button>(Resource.Id.btnStatistics);
            button.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(StatisticsActivity));
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
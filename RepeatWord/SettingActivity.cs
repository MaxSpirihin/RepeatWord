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
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

namespace RepeatWord
{
    [Activity(Label = "SettingActivity")]
    public class SettingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_setting);

            var numberPicker = FindViewById<EditText>(Resource.Id.etSkipLastWords);
            numberPicker.Text = "100";

            Button button = FindViewById<Button>(Resource.Id.btnPickFile);
            button.Click += async (sender, e) =>
            {
                try
                {
                    FileData fileData = await CrossFilePicker.Current.PickFile();
                    if (fileData == null)
                    {
                        Toast.MakeText(Application.Context, "Cant get file", ToastLength.Long).Show();
                        return;
                    }
                    
                    string fileName = fileData.FileName;

                    Console.WriteLine("File name chosen: " + fileName);
                    string er;
                    List<Word> words = ExcelParser.Parse(fileData.DataArray.ToArray(), out er);
                    
                    if (words.Count > 0)
                    {
                        words = words.Take(words.Count - Convert.ToInt32(FindViewById<EditText>(Resource.Id.etSkipLastWords).Text)).ToList();
                        WordsManager.Instance.SetWords(words);

                        Toast.MakeText(Application.Context, "Updated " + words.Count + " words", ToastLength.Long).Show();
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, "Parse ex " + er, ToastLength.Long).Show();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception choosing file: " + ex.ToString());
                    Toast.MakeText(Application.Context, "Load ex " + ex.Message, ToastLength.Long).Show();
                }
            };

            button = FindViewById<Button>(Resource.Id.btnResetCurrentFullSession);
            button.Click += (sender, e) =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Confirm");
                alert.SetMessage("Reset Current Full Session?");
                alert.SetPositiveButton("Reset", (senderAlert, args) => {
                    WordsManager.Instance.ResetFullSession();
                    Toast.MakeText(this, "Reseted!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            };

            button = FindViewById<Button>(Resource.Id.btnResetCurrentLearnSession);
            button.Click += (sender, e) =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Confirm");
                alert.SetMessage("Reset Current Learn Session?");
                alert.SetPositiveButton("Reset", (senderAlert, args) => {
                    WordsManager.Instance.ResetLearnSession();
                    Toast.MakeText(this, "Reseted!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            };

        }
    }
}
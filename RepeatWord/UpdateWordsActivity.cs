using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using static Android.App.ActionBar;
using Xamarin.Essentials;

namespace RepeatWord
{
    [Activity(Label = "UpdateWordsActivity")]
    public class UpdateWordsActivity : Activity
    {
        TextView m_TextView;
        StringBuilder m_Builder;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_log);
            m_TextView = FindViewById<TextView>(Resource.Id.textViewLog);
            m_Builder = new StringBuilder();
            
            Load();
        }

        private async void Load()
        {
            AddText("Loading has started. Please do not close the page while loading is in process.");
            Exception exception = null;
            string processingURL = null;
            bool atLeastOneListProcessed = false;
            List<Word> words = new List<Word>();

            try
            {
                HttpClient client = new HttpClient();
               
                for (int i = 1; ; i++)
                {
                    if (IsDestroyed)
                    {
                        Toast.MakeText(Application.Context, "Loading was interrupted and was not completed", ToastLength.Long).Show();
                        return;
                    }

                    processingURL = $"https://spreadsheets.google.com/feeds/cells/{WordsManager.Instance.Data.GoogleSheetID}/{i}/public/full?alt=json";
                    var response = await client.GetAsync(processingURL);
                    string result = await response.Content.ReadAsStringAsync();

                    bool isJson = result.StartsWith("{");

                    if (!isJson)
                    {
                        if (!atLeastOneListProcessed)
                        {
                            AddText("Url data has incorrect Format");
                            AddText("URL = " + processingURL);
                            AddText("Result = " + result);
                        }
                        break;
                    }
                    
                    atLeastOneListProcessed = true;
                    List<Word> wordsFromOneList = WordsParser.ParseFromGoogleSheetJson(result, i, out exception);

                    if (exception != null)
                        break;

                    words.AddRange(wordsFromOneList);

                    AddText($"List {i} loaded.");
                    AddText($"Total {words.Count} words loaded.");
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception == null && atLeastOneListProcessed)
            {
                AddText("LoadingCompleted");
                AddText("Start updating database");

                if (words.Count > 0)
                    WordsManager.Instance.SetWords(words);

                AddText($"Successfully update {words.Count} words.");
            }
            
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.linLayoutLog);
            LayoutParams lp = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);

            if (exception != null)
            {
                AddText("An error ocurred");
                AddText(exception.ToString());
                AddText("ERROR URL = " + processingURL);

                Button cbButton = new Button(this);
                cbButton.Text = "Copy log to clipboard";
                layout.AddView(cbButton, lp);
                cbButton.Click += (sender, e) =>
                {
                    Clipboard.SetTextAsync(m_TextView.Text);
                };
            }

            Button backButton = new Button(this);
            backButton.Text = "Back";
            layout.AddView(backButton, lp);
            backButton.Click += (sender, e) =>
            {
                base.OnBackPressed();
            };
        }

        void AddText(string _Text)
        {
            m_Builder.AppendLine(_Text);
            m_TextView.Text = m_Builder.ToString();
        }
    }
}
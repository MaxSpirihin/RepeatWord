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
        EditText m_TextActiveLearnCount;
        EditText m_TextDailyRepeatCount;
        EditText m_NewWordList;
        EditText m_NewWordRow;
        EditText m_GoogleSheetID;
        Button m_NextButton;
        TextView m_CurrentWords;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_setting);

            m_TextActiveLearnCount = FindViewById<EditText>(Resource.Id.etLearnWordsCount);
            m_TextDailyRepeatCount = FindViewById<EditText>(Resource.Id.etDailyWordsCount);
            m_NewWordList = FindViewById<EditText>(Resource.Id.etListNum);
            m_NewWordRow = FindViewById<EditText>(Resource.Id.etRowNum);
            m_GoogleSheetID = FindViewById<EditText>(Resource.Id.etGoogleSheetID);
            m_CurrentWords = FindViewById<TextView>(Resource.Id.tvCurrentWords);

            m_NextButton = FindViewById<Button>(Resource.Id.btnMoveToNextWords);
            m_NextButton.Click += (sender, e) =>
            {
                List<Word> result = new List<Word>();
                int? nextNewWordIndex = WordsManager.Instance.Data.GetFirstNewWordIndex();

                if (nextNewWordIndex != null && nextNewWordIndex.Value + WordsManager.Instance.Data.ActiveLearnCount < WordsManager.Instance.Data.Words.Count)
                {
                    Word nextWord = WordsManager.Instance.Data.Words[nextNewWordIndex.Value + WordsManager.Instance.Data.ActiveLearnCount];
                    m_NewWordList.Text = nextWord.ListNum.ToString();
                    m_NewWordRow.Text = nextWord.Row.ToString();
                }
                else
                {
                    Toast.MakeText(Application.Context, "You have learnt all the words in sheet. Update it", ToastLength.Long).Show();
                }
            };

            m_TextActiveLearnCount.Text = WordsManager.Instance.Data.ActiveLearnCount.ToString();
            m_TextDailyRepeatCount.Text = WordsManager.Instance.Data.DailyRepeatCount.ToString();
            m_NewWordList.Text = WordsManager.Instance.Data.NewWordList.ToString();
            m_NewWordRow.Text = WordsManager.Instance.Data.NewWordRow.ToString();
            m_GoogleSheetID.Text = WordsManager.Instance.Data.GoogleSheetID;

            m_TextActiveLearnCount.TextChanged += TextChanged;
            m_TextDailyRepeatCount.TextChanged += TextChanged;
            m_NewWordList.TextChanged += TextChanged;
            m_NewWordRow.TextChanged += TextChanged;
            m_GoogleSheetID.TextChanged += TextChanged;

            TextChanged(null, null);

            PrepareButtons();
        }

        private void TextChanged(object _Sender, Android.Text.TextChangedEventArgs _E)
        {
            WordsManager.Instance.Data.ActiveLearnCount = GetIntValue(m_TextActiveLearnCount, 0);
            
            int dailyRepeatCountOld = WordsManager.Instance.Data.DailyRepeatCount;
            int newWordListOld = WordsManager.Instance.Data.NewWordList;
            int newWordRowOld = WordsManager.Instance.Data.NewWordRow;

            WordsManager.Instance.Data.DailyRepeatCount = GetIntValue(m_TextDailyRepeatCount, 0);
            WordsManager.Instance.Data.NewWordList = GetIntValue(m_NewWordList, 0);
            WordsManager.Instance.Data.NewWordRow = GetIntValue(m_NewWordRow, 0);
            WordsManager.Instance.Data.GoogleSheetID = m_GoogleSheetID.Text;

            if (WordsManager.Instance.Data.DailyRepeatCount != dailyRepeatCountOld || 
                WordsManager.Instance.Data.NewWordList != newWordListOld ||
                WordsManager.Instance.Data.NewWordRow != newWordRowOld)
            {
                WordsManager.Instance.Data.CurrentSessions.Remove(RepeatSessionType.DAILY_REPEAT);
                WordsManager.Instance.Data.CurrentSessions.Remove(RepeatSessionType.DAILY_REPEAT_RANDOM);
            }

            WordsManager.Instance.SaveDataToCache();

            m_NextButton.Text = "+" + m_TextActiveLearnCount.Text;

            m_CurrentWords.Text = "Active words : " + string.Join(", ", WordsManager.Instance.Data.GetActiveLearnWords().Select(_W => _W.English).ToArray());
        }

        int GetIntValue(EditText _EditText, int _Default)
        {
            if (string.IsNullOrEmpty(_EditText.Text))
                return _Default;

            return Convert.ToInt32(_EditText.Text);
        }

        void PrepareButtons()
        {
            Button button = FindViewById<Button>(Resource.Id.btnPickFile);
            button.Click += (sender, e) =>
            {
                StartActivity(new Intent(this, typeof(UpdateWordsActivity)));
            };

            button = FindViewById<Button>(Resource.Id.btnResetCurrentFullSession);
            button.Click += (sender, e) =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Confirm");
                alert.SetMessage("Reset Current Full Session?");
                alert.SetPositiveButton("Reset", (senderAlert, args) => {
                    WordsManager.Instance.Data.CurrentSessions.Remove(RepeatSessionType.FULL_REPEAT);
                    WordsManager.Instance.SaveDataToCache();
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
                alert.SetMessage("Reset Current Repeat forgotten Session?");
                alert.SetPositiveButton("Reset", (senderAlert, args) => {
                    WordsManager.Instance.Data.CurrentSessions.Remove(RepeatSessionType.REPEAT_FORGOTTEN);
                    WordsManager.Instance.SaveDataToCache();
                    Toast.MakeText(this, "Reseted!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            };

            button = FindViewById<Button>(Resource.Id.btnResetCurrentDailySession);
            button.Click += (sender, e) =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Confirm");
                alert.SetMessage("Reset Current Daily Sessions?");
                alert.SetPositiveButton("Reset", (senderAlert, args) => {
                    WordsManager.Instance.Data.CurrentSessions.Remove(RepeatSessionType.DAILY_REPEAT);
                    WordsManager.Instance.Data.CurrentSessions.Remove(RepeatSessionType.DAILY_REPEAT);
                    WordsManager.Instance.SaveDataToCache();
                    Toast.MakeText(this, "Reseted!", ToastLength.Short).Show();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            };
        }
    }
}
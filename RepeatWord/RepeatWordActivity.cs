using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Speech.Tts;
using Android.Widget;

namespace RepeatWord
{
    [Activity(Label = "RepeatWordActivity")]
    public class RepeatWordActivity : Activity
    {
        const int LIST_WORDS_COUNT = 20;

        List<ListItemWord> m_ListItemWords = new List<ListItemWord>();
        ListView m_ListView;
        ListItemWordAdapter m_ListViewAdapter;
        RepeatSession m_Session;
        Button m_NextButton;

        DateTime m_StartCurrentPageTime;
        DateTime? m_PauseTime;
        int m_SecondsInPause;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_repeat_word);

            RepeatSessionType type = (RepeatSessionType)Intent.Extras.GetInt("Type", 0);
            m_ListView = FindViewById<ListView>(Resource.Id.lwMain);
            
            m_Session = WordsManager.Instance.GetOrGenerateCurrentSession(type);

            if (m_Session == null)
            {
                Toast.MakeText(Application.Context, "Cant generate session", ToastLength.Long).Show();
                Finish();
                return;
            }

            if (m_Session.Words.Count == 0)
            {
                Toast.MakeText(Application.Context, "No words for that sesion", ToastLength.Long).Show();
                Finish();
                return;
            }

            m_ListView.TextFilterEnabled = true;
            m_ListView.Adapter = m_ListViewAdapter = new ListItemWordAdapter(this, m_ListItemWords);
            m_ListView.ItemClick += OnListItemClick;
            
            m_NextButton = FindViewById<Button>(Resource.Id.btnGoNext);
            m_NextButton.Click += (sender, e) =>
            {
                //because user can fast press 2 times
                if (m_Session.RepeatedWords >= m_Session.Words.Count)
                {
                    Finish();
                    return;
                }

                for (int i = 0; i< m_ListItemWords.Count; i++)
                {
                    m_Session.Words[m_Session.RepeatedWords + i].IsForgotten = m_ListItemWords[i].IsForgotten;
                }
                m_Session.LearnSeconds += (int)(DateTime.Now - m_StartCurrentPageTime).TotalSeconds - m_SecondsInPause;
                m_Session.RepeatedWords += m_ListItemWords.Count;
                WordsManager.Instance.SaveDataToCache();
                PrepareWords();
            };

            
            
            PrepareWords();
        }

        protected override void OnPause()
        {
            base.OnPause();

            m_PauseTime = DateTime.Now;
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (m_PauseTime.HasValue)
            {
                m_SecondsInPause += (int)(DateTime.Now - m_PauseTime.Value).TotalSeconds;
                m_PauseTime = null;
            }
        }

        void PrepareWords()
        {
            if (m_Session.RepeatedWords >= m_Session.Words.Count)
            {
                Finish();
                return;
            }
            int forggotten = m_Session.Words.Take(m_Session.RepeatedWords).Count(_W => _W.IsForgotten);
            int forgottenPercent = m_Session.RepeatedWords > 0 ? forggotten * 100 / m_Session.RepeatedWords : 0;
            m_NextButton.Text = string.Format(
                "Next ({0}/{1}) Forget - ({2},{3}%), Time - {4}:{5}",
                m_Session.RepeatedWords,
                m_Session.Words.Count,
                forggotten,
                forgottenPercent,
                m_Session.LearnSeconds / 60,
                m_Session.LearnSeconds % 60
            );

            m_ListItemWords.Clear();
            for (int i = m_Session.RepeatedWords; i < Math.Min(m_Session.RepeatedWords + LIST_WORDS_COUNT, m_Session.Words.Count); i++)
            {
                RepeatSessionWord word = m_Session.Words[i];
                m_ListItemWords.Add(new ListItemWord() { Russian = WordsManager.Instance.GetWord(word.English).Russian, English = word.English });
            }
            m_ListViewAdapter.NotifyDataSetChanged();

            m_StartCurrentPageTime = DateTime.Now;
            m_SecondsInPause = 0;
            m_PauseTime = null;
        }

        void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var t = m_ListItemWords[e.Position];

            if (t.ShowRussian)
                t.IsForgotten = !t.IsForgotten;
            else
                t.ShowRussian = true;

            m_ListViewAdapter.NotifyDataSetChanged();
        }

       
    }
}
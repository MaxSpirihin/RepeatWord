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
            m_ListView.Adapter = m_ListViewAdapter = new ListItemWordAdapter(this, m_ListItemWords, type.NeedReverse());
            m_ListView.ItemClick += OnListItemClick;
            
            m_NextButton = FindViewById<Button>(Resource.Id.btnGoNext);
            m_NextButton.Click += (sender, e) =>
            {
                NextButtonClicked();
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

        void NextButtonClicked()
        {
            //because user can fast press 2 times
            if (m_Session.IsEnded)
            {
                Finish();
                return;
            }

            if (m_Session.ImmediateRepeatIsInProgress)
            {
                foreach (ListItemWord listItemWord in m_ListItemWords.Where(_W => _W.IsForgotten))
                    m_Session.AddForgottenWordInImmediateRepeat(listItemWord.English);

                //stop immediate repeat only when user made no mistake in it
                if (m_ListItemWords.All(_W => !_W.IsForgotten))
                    m_Session.ImmediateRepeatIsInProgress = false;
            }
            else if (m_Session.PostRepeatIsInProgress)
            {
                for (int i = 0; i < m_ListItemWords.Count; i++)
                    m_Session.PostRepeatWords[m_Session.RepeatedWordsInPostRepeat + i].IsForgotten = m_ListItemWords[i].IsForgotten;

                m_Session.RepeatedWordsInPostRepeat += m_ListItemWords.Count;
            }
            else
            {
                for (int i = 0; i < m_ListItemWords.Count; i++)
                    m_Session.Words[m_Session.RepeatedWords + i].IsForgotten = m_ListItemWords[i].IsForgotten;

                m_Session.RepeatedWords += m_ListItemWords.Count;

                if (m_Session.RepeatSessionType.SupportImmediateRepeat() && m_ListItemWords.Any(_W => _W.IsForgotten))
                {
                    //activate ImmediateRepeat for next update
                    m_Session.ImmediateRepeatIsInProgress = true;
                }
            }

            if (m_Session.IsEnded && !m_Session.PostRepeatIsInProgress && m_Session.RepeatSessionType.SupportPostRepeat())
                m_Session.PreparePostRepeat();

            m_Session.LearnSeconds += (int)(DateTime.Now - m_StartCurrentPageTime).TotalSeconds - m_SecondsInPause;
            WordsManager.Instance.SaveDataToCache();
            PrepareWords();
        }

        void PrepareWords()
        {
            if (m_Session.IsEnded)
            {
                Finish();
                return;
            }

            m_NextButton.Text = GetTextForButton();

            m_ListItemWords.Clear();

            if (m_Session.ImmediateRepeatIsInProgress)
            {
                for (int i = m_Session.RepeatedWords - LIST_WORDS_COUNT; i < m_Session.RepeatedWords; i++)
                {
                    RepeatSessionWord word = m_Session.Words[i];
                    if (word.IsForgotten)
                        m_ListItemWords.Add(new ListItemWord() { Russian = WordsManager.Instance.GetWord(word.English).Russian, English = word.English });
                }
            }
            else
            {
                List<RepeatSessionWord> words = m_Session.PostRepeatIsInProgress ? m_Session.PostRepeatWords : m_Session.Words;
                int repeatedCount = m_Session.PostRepeatIsInProgress ? m_Session.RepeatedWordsInPostRepeat : m_Session.RepeatedWords;

                for (int i = repeatedCount; i < Math.Min(repeatedCount + LIST_WORDS_COUNT, words.Count); i++)
                {
                    RepeatSessionWord word = words[i];
                    m_ListItemWords.Add(new ListItemWord() { Russian = WordsManager.Instance.GetWord(word.English).Russian, English = word.English });
                }
            }
            
            m_ListViewAdapter.NotifyDataSetChanged();
            m_ListView.SetSelectionAfterHeaderView();

            m_StartCurrentPageTime = DateTime.Now;
            m_SecondsInPause = 0;
            m_PauseTime = null;
        }

        string GetTextForButton()
        {
            if (m_Session.ImmediateRepeatIsInProgress)
                return "Immediate Repeat";

            List<RepeatSessionWord> words = m_Session.PostRepeatIsInProgress ? m_Session.PostRepeatWords : m_Session.Words;
            int repeatedCount = m_Session.PostRepeatIsInProgress ? m_Session.RepeatedWordsInPostRepeat : m_Session.RepeatedWords;

            int forggotten = words.Take(m_Session.RepeatedWords).Count(_W => _W.IsForgotten);
            int forgottenPercent = repeatedCount > 0 ? forggotten * 100 / repeatedCount : 0;
            
            return string.Format(
                    "{0}{1}/{2} : F - ({3},{4}%) : T - {5}:{6}",
                    m_Session.PostRepeatIsInProgress ? "PR " : string.Empty,
                    repeatedCount,
                    words.Count,
                    forggotten,
                    forgottenPercent,
                    m_Session.LearnSeconds / 60,
                    m_Session.LearnSeconds % 60
                );
        }

        void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var t = m_ListItemWords[e.Position];

            if (t.ShowTranslate)
                t.IsForgotten = !t.IsForgotten;
            else
                t.ShowTranslate = true;

            m_ListViewAdapter.NotifyDataSetChanged();
        }
    }
}
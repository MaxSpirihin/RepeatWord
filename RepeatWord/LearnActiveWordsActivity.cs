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
    [Activity(Label = "LearnActiveWordsActivity")]
    public class LearnActiveWordsActivity : Activity
    {
        List<ListItemWord> m_ListItemWords = new List<ListItemWord>();
        ListView m_ListView;
        ListItemWordAdapter m_ListViewAdapter;
        RepeatSession m_Session;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_learn_active_words);
            m_Session = WordsManager.Instance.GetActiveLearnSession();
            
            if (m_Session == null)
            {
                Toast.MakeText(Application.Context, "Cant generate session", ToastLength.Long).Show();
                Finish();
                return;
            }

            m_ListView = FindViewById<ListView>(Resource.Id.lwMain);
            m_ListView.TextFilterEnabled = true;
            m_ListView.Adapter = m_ListViewAdapter = new ListItemWordAdapter(this, m_ListItemWords, false);
            m_ListView.ItemClick += OnListItemClick;
            
            FindViewById<Button>(Resource.Id.btnNewWordsShowAll).Click += (sender, e) =>
            {
                foreach (var word in m_ListItemWords)
                    word.ShowTranslate = true;
                m_ListViewAdapter.NotifyDataSetChanged();
            };

            FindViewById<Button>(Resource.Id.btnNewWordsClear).Click += (sender, e) =>
            {
                foreach (var word in m_ListItemWords)
                    word.ShowTranslate = false;
                m_ListViewAdapter.NotifyDataSetChanged();
            };

            foreach (RepeatSessionWord word in m_Session.Words)
            {
                m_ListItemWords.Add(new ListItemWord() { Russian = WordsManager.Instance.GetWord(word.English).Russian, English = word.English });
            }
            m_ListViewAdapter.NotifyDataSetChanged();
        }
        
        void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var t = m_ListItemWords[e.Position];
            t.ShowTranslate = !t.ShowTranslate;
            m_ListViewAdapter.NotifyDataSetChanged();
        }

       
    }
}
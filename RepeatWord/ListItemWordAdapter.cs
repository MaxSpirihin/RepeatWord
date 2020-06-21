using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;

namespace RepeatWord
{
    public class ListItemWord
    {
        public string English;
        public string Russian;
        public bool IsForgotten;
        public bool ShowTranslate;
    }

    public class ListItemWordAdapter : BaseAdapter<ListItemWord>, TextToSpeech.IOnInitListener
    {
        List<ListItemWord> items;
        Activity context;
        TextToSpeech textToSpeech;
        bool m_ReverseTranslation;

        //i know this is just garbage hack. But i have no time find other way
        Dictionary<View, string> m_ButtonBindings = new Dictionary<View, string>();

        public ListItemWordAdapter(Activity _Context, List<ListItemWord> _Items, bool _ReverseTranslation)
            : base()
        {
            this.context = _Context;
            this.items = _Items;
            m_ReverseTranslation = _ReverseTranslation;
            textToSpeech = new TextToSpeech(_Context, this, "com.google.android.tts");
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override ListItemWord this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;

            bool createView = view == null;
            if (createView)
                view = context.LayoutInflater.Inflate(Resource.Layout.list_item_word, null);
            
            view.FindViewById<TextView>(Resource.Id.EnglishWord).Text = m_ReverseTranslation ? item.Russian : item.English;
            view.FindViewById<TextView>(Resource.Id.RussianWord).Text = item.ShowTranslate 
                ? m_ReverseTranslation ? item.English : item.Russian
                : String.Empty;
            view.SetBackgroundColor(item.IsForgotten ? Android.Graphics.Color.LightPink : Android.Graphics.Color.White);

            Button btnSpeak = view.FindViewById<Button>(Resource.Id.SpeakButton);
            Button btnWeb = view.FindViewById<Button>(Resource.Id.WebButton);

            m_ButtonBindings[view] = item.English;

            if (createView)
            {
                btnSpeak.Click += (sender, e) =>
                {
                    string bind = m_ButtonBindings[view];
                    textToSpeech.Speak(bind, QueueMode.Flush, null);
                };
                btnWeb.Click += (sender, e) =>
                {
                    string bind = m_ButtonBindings[view];
                    var uri = Android.Net.Uri.Parse("https://context.reverso.net/перевод/английский-русский/" + bind);
                    var intent = new Intent(Intent.ActionView, uri);
                    context.StartActivity(intent);
                };
            }

            return view;
        }

        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {
            textToSpeech.SetLanguage(Java.Util.Locale.English);
        }
    }
}
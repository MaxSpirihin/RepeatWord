using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
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
        public bool ShowRussian;
    }

    public class ListItemWordAdapter : BaseAdapter<ListItemWord>, TextToSpeech.IOnInitListener
    {
        List<ListItemWord> items;
        Activity context;
        TextToSpeech textToSpeech;

        public ListItemWordAdapter(Activity context, List<ListItemWord> items)
            : base()
        {
            this.context = context;
            this.items = items;
            textToSpeech = new TextToSpeech(context, this, "com.google.android.tts");
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
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.list_item_word, null);
            view.FindViewById<TextView>(Resource.Id.EnglishWord).Text = item.English;
            view.FindViewById<TextView>(Resource.Id.RussianWord).Text = item.ShowRussian ? item.Russian : String.Empty;
            view.SetBackgroundColor(item.IsForgotten ? Android.Graphics.Color.LightPink : Android.Graphics.Color.White);

            view.FindViewById<Button>(Resource.Id.SpeakButton).Click += (sender, e) =>
            {
                textToSpeech.Speak(item.English, QueueMode.Flush, null);
            };
            return view;
        }

        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {
            textToSpeech.SetLanguage(Java.Util.Locale.English);
        }
    }
}
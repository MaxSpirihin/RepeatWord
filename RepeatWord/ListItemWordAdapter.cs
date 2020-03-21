﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
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

    public class ListItemWordAdapter : BaseAdapter<ListItemWord>
    {
        List<ListItemWord> items;
        Activity context;
        public ListItemWordAdapter(Activity context, List<ListItemWord> items)
            : base()
        {
            this.context = context;
            this.items = items;
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
            return view;
        }
    }
}
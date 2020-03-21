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

namespace RepeatWord
{
    public class RepeatSessionWord
    {
        public string English;
        public bool IsForgotten;
    }

    public class RepeatSession
    {
        public RepeatSession(bool _IsFull)
        {
            Words = new List<RepeatSessionWord>();
            IsFull = _IsFull;
        }

        public List<RepeatSessionWord> Words;
        public int RepeatedWords;
        public bool IsFull;

        public void AddWord(Word _Word)
        {
            Words.Add(new RepeatSessionWord()
            {
                English = _Word.English
            });
        }

        public void AddWord(string English)
        {
            Words.Add(new RepeatSessionWord()
            {
                English = English
            });
        }
    }
}
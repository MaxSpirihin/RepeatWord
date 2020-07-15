using System;
using System.Collections.Generic;


namespace RepeatWord
{
    public class WordsData
    {
        public List<Word> Words;
        public List<RepeatSession> CompletedSessions;
        public Dictionary<RepeatSessionType, RepeatSession> CurrentSessions;
        public int NewWordList;
        public int NewWordRow;
        public int DailyRepeatCount;
        public int ActiveLearnCount;
        public string GoogleSheetID;

        public WordsData()
        {
            Words = new List<Word>();
            CompletedSessions = new List<RepeatSession>();
            CurrentSessions = new Dictionary<RepeatSessionType, RepeatSession>();
        }

        public int? GetFirstNewWordIndex()
        {
            List<Word> result = new List<Word>();
            for (int i = 0; i < Words.Count; i++)
            {
                if (Words[i].ListNum > NewWordList || Words[i].ListNum == NewWordList && Words[i].Row >= NewWordRow)
                    return i;
            }

            return null;
        }

        public List<Word> GetActiveLearnWords()
        {
            List<Word> result = new List<Word>();
            int? index = GetFirstNewWordIndex();
            if (index == null)
                return result;

            for (int i = index.Value; i < Math.Min(Words.Count, index.Value + ActiveLearnCount); i++)
            {
                result.Add(Words[i]);
            }

            return result;
        }

        public List<Word> GetLearntWords()
        {
            List<Word> result = new List<Word>();
            int? index = GetFirstNewWordIndex();
            if (index == null)
                return result;

            for (int i = 0; i < index.Value - DailyRepeatCount; i++)
            {
                result.Add(Words[i]);
            }

            return result;
        }

        public List<Word> GetDailyRepeatWords()
        {
            List<Word> result = new List<Word>();
            int? index = GetFirstNewWordIndex();
            if (index == null)
                return result;

            for (int i = Math.Max(0, index.Value - DailyRepeatCount); i < index.Value; i++)
            {
                result.Add(Words[i]);
            }

            return result;
        }
    }
}
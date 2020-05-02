using System.Collections.Generic;

namespace RepeatWord
{
    public enum RepeatSessionType
    {
        FULL_REPEAT = 0,
        REPEAT_FORGOTTEN = 1,
        DAILY_REPEAT = 2,
        ACTIVE_LEARN = 3,
        DAILY_REPEAT_RANDOM = 4,
    }

    public class RepeatSessionWord
    {
        public string English;
        public bool IsForgotten;
    }

    public class RepeatSession
    {
        public RepeatSession(RepeatSessionType _RepeatSessionType)
        {
            Words = new List<RepeatSessionWord>();
            RepeatSessionType = _RepeatSessionType;
        }

        public List<RepeatSessionWord> Words;
        public int RepeatedWords;
        public RepeatSessionType RepeatSessionType;
        public int LearnSeconds;

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
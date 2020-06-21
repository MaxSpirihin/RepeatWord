using System;
using System.Collections.Generic;
using System.Linq;

namespace RepeatWord
{
    public enum RepeatSessionType
    {
        FULL_REPEAT = 0,
        REPEAT_FORGOTTEN = 1,
        DAILY_REPEAT = 2,
        ACTIVE_LEARN = 3,
        DAILY_REPEAT_RANDOM = 4,
        FULL_REPEAT_REVERSED = 5,
    }

    public static class RepeatSessionTypeExtension
    {
        public static bool NeedReverse(this RepeatSessionType _Type)
        {
            return _Type == RepeatSessionType.FULL_REPEAT_REVERSED;
        }

        public static bool SupportImmediateRepeat(this RepeatSessionType _Type)
        {
            return _Type == RepeatSessionType.REPEAT_FORGOTTEN;
        }

        public static bool SupportPostRepeat(this RepeatSessionType _Type)
        {
            return _Type == RepeatSessionType.REPEAT_FORGOTTEN || _Type == RepeatSessionType.DAILY_REPEAT || _Type == RepeatSessionType.DAILY_REPEAT_RANDOM;
        }
    }

    public class RepeatSessionWord
    {
        public string English;
        public bool IsForgotten;
    }

    public class RepeatSession
    {
        #region fields

        public DateTime Date;

        public RepeatSessionType RepeatSessionType;
        public List<RepeatSessionWord> Words;

        public int RepeatedWords;

        public bool ImmediateRepeatIsInProgress;
        public List<string> ForgottenInImmediateRepeat;

        public int RepeatedWordsInPostRepeat;
        public List<RepeatSessionWord> PostRepeatWords;

        public int LearnSeconds;

        #endregion

        #region properties

        public bool PostRepeatIsInProgress => PostRepeatWords != null;

        public bool IsEnded
        {
            get
            {
                if (ImmediateRepeatIsInProgress)
                    return false;

                if (PostRepeatWords != null)
                    return RepeatedWordsInPostRepeat >= PostRepeatWords.Count;

                return RepeatedWords >= Words.Count;
            }
        }

        #endregion

        #region constructor

        public RepeatSession(RepeatSessionType _RepeatSessionType)
        {
            Date = DateTime.Now;
            Words = new List<RepeatSessionWord>();
            RepeatSessionType = _RepeatSessionType;
        }

        #endregion

        #region public methods

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

        public void AddForgottenWordInImmediateRepeat(string English)
        {
            if (ForgottenInImmediateRepeat == null)
                ForgottenInImmediateRepeat = new List<string>();

            ForgottenInImmediateRepeat.Add(English);
        }

        public void PreparePostRepeat()
        {
            if (PostRepeatWords != null)
                return;

            PostRepeatWords = Words.Where(_W => _W.IsForgotten).Select(_W => new RepeatSessionWord() { English = _W.English }).ToList();
        }

        #endregion
    }
}
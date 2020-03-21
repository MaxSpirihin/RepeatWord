using System.Collections.Generic;


namespace RepeatWord
{
    public class WordsData
    {
        public List<Word> Words;
        public List<RepeatSession> CompletedSessions;
        public RepeatSession CurrentFullSession;
        public RepeatSession CurrentLearnSession;

        public WordsData()
        {
            Words = new List<Word>();
            CompletedSessions = new List<RepeatSession>();
        }
    }
}
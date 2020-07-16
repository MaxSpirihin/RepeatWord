using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RepeatWord
{
    public class WordsManager
    {
        #region singleton

        static WordsManager m_Instance;

        public static WordsManager Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new WordsManager();

                return m_Instance;
            }
        }

        private WordsManager() { }

        #endregion

        #region attributes
        
        Dictionary<string, Word> m_WordsCache;

        #endregion

        #region properties

        string RootFolderPath { get; set; }

        public WordsData Data { get; private set; }

        string LegacyCacheFilePath
        {
            get { return Path.Combine(RootFolderPath, "RepeatWord", "RepeatWordData.txt"); }
        }

        string CacheFolderPath
        {
            get { return Path.Combine(RootFolderPath, "FastVocab"); } 
        }

        string CacheFilePath
        {
            get { return Path.Combine(CacheFolderPath, "FastVocabData.txt"); }
        }

        #endregion

        public void Init(string _FolderForCache)
        {
            RootFolderPath = _FolderForCache;
            string json = GetJsonFromCache();

            Data = string.IsNullOrEmpty(json) ? new WordsData() : JsonConvert.DeserializeObject<WordsData>(json);
            
            UpdateWordsCache();
        }

        public void SetWords(List<Word> _Words)
        {
            Data.Words = _Words;
            SaveDataToCache();
            UpdateWordsCache();
        }
        
        public RepeatSession GetActiveLearnSession()
        {
            RepeatSession session = new RepeatSession(RepeatSessionType.ACTIVE_LEARN);
            
            foreach (var word in Data.GetActiveLearnWords())
                session.AddWord(word.English);

            return session;
        }

        public RepeatSession GetOrGenerateCurrentSession(RepeatSessionType _Type)
        {
            RepeatSession session;
            if (Data.CurrentSessions.TryGetValue(_Type, out session) && session != null)
                return Data.CurrentSessions[_Type];

            session = new RepeatSession(_Type);
            Random random = new Random();
            switch (_Type)
            {
                case RepeatSessionType.FULL_REPEAT:
                case RepeatSessionType.FULL_REPEAT_REVERSED:
                    foreach (Word word in Data.GetLearntWords().OrderBy(_W => random.Next()))
                        session.AddWord(word);
                    break;
                case RepeatSessionType.REPEAT_FORGOTTEN:
                    RepeatSession lastFullSession = Data.CompletedSessions.
                        LastOrDefault(_S => _S.RepeatSessionType == RepeatSessionType.FULL_REPEAT);

                    if (lastFullSession == null)
                        return null;
                    
                    foreach (var word in lastFullSession.Words.Where(_W => _W.IsForgotten).OrderBy(_W => random.Next()))
                        session.AddWord(word.English);

                    break;
                case RepeatSessionType.DAILY_REPEAT:
                    foreach (var word in Data.GetDailyRepeatWords())
                        session.AddWord(word.English);
                    break;
                case RepeatSessionType.DAILY_REPEAT_RANDOM:
                    foreach (var word in Data.GetDailyRepeatWords().OrderBy(_W => random.Next()))
                        session.AddWord(word.English);
                    break;
                case RepeatSessionType.ACTIVE_LEARN:
                    return null;
            }
            
            Data.CurrentSessions[_Type] = session;
            SaveDataToCache();
            return session;
        }
        
        public void SaveDataToCache()
        {
            foreach (var session in Data.CurrentSessions.ToList())
            {
                if (session.Value != null && session.Value.IsEnded)
                {
                    Data.CompletedSessions.Add(session.Value);
                    Data.CurrentSessions[session.Key] = null;
                }
            }

            if (!Directory.Exists(CacheFolderPath))
                Directory.CreateDirectory(CacheFolderPath);

            File.WriteAllText(CacheFilePath, JsonConvert.SerializeObject(Data));
        }

        public Word GetWord(string _English)
        {
            return m_WordsCache[_English];
        }

        #region service methods

        void UpdateWordsCache()
        {
            m_WordsCache = new Dictionary<string, Word>();
            foreach (Word word in Data.Words)
                m_WordsCache[word.English] = word;
        }

        string GetJsonFromCache()
        {
            if (!Directory.Exists(CacheFolderPath))
                Directory.CreateDirectory(CacheFolderPath);

            //check legacy folder
            if (!File.Exists(CacheFilePath) && File.Exists(LegacyCacheFilePath))
                File.Copy(LegacyCacheFilePath, CacheFilePath);

            return File.Exists(CacheFilePath) ? File.ReadAllText(CacheFilePath) : string.Empty;
        }

        #endregion
    }
}
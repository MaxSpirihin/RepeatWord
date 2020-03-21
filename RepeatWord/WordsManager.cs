using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

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

        WordsData m_Data;
        Dictionary<string, Word> m_WordsCache;

        #endregion

        #region properties

        string CacheFolderName
        {
            get
            {
                return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "RepeatWord");
            }
        }

        string CacheFileName
        {
            get
            {
                return Path.Combine(CacheFolderName, "RepeatWordData.txt");
            }
        }

        #endregion

        public void Init()
        {
            string json = GetJsonFromCache();

            m_Data = string.IsNullOrEmpty(json) ? new WordsData() : JsonConvert.DeserializeObject<WordsData>(json);
            UpdateWordsCache();
        }

        public void SetWords(List<Word> _Words)
        {
            m_Data.Words = _Words;
            SaveDataToCache();
            UpdateWordsCache();
        }

        public List<Word> GetAllWords()
        {
            return m_Data.Words;
        }

        public RepeatSession GetOrGenerateFullSession()
        {
            if (m_Data.CurrentFullSession != null)
                return m_Data.CurrentFullSession;

            RepeatSession session = new RepeatSession(true);

            Random random = new Random();
            foreach (Word word in m_Data.Words.OrderBy(_W => random.Next()))
                session.AddWord(word);

            m_Data.CurrentFullSession = session;
            SaveDataToCache();
            return session;
        }

        public RepeatSession GetOrGenerateLearnSession()
        {
            if (m_Data.CurrentLearnSession != null)
                return m_Data.CurrentLearnSession;

            RepeatSession lastFullSession = m_Data.CompletedSessions.LastOrDefault(_S => _S.IsFull);

            if (lastFullSession == null)
                return null;

            RepeatSession session = new RepeatSession(false);
            
            foreach (var word in lastFullSession.Words.Where(_W => _W.IsForgotten))
                session.AddWord(word.English);

            m_Data.CurrentLearnSession = session;
            SaveDataToCache();
            return session;
        }

        public void ResetFullSession()
        {
            m_Data.CurrentFullSession = null;
            SaveDataToCache();
        }

        public void ResetLearnSession()
        {
            m_Data.CurrentLearnSession = null;
            SaveDataToCache();
        }

        public void SaveDataToCache()
        {
            if (m_Data.CurrentFullSession != null && m_Data.CurrentFullSession.RepeatedWords >= m_Data.CurrentFullSession.Words.Count)
            {
                m_Data.CompletedSessions.Add(m_Data.CurrentFullSession);
                m_Data.CurrentFullSession = null;
            }

            if (m_Data.CurrentLearnSession != null && m_Data.CurrentLearnSession.RepeatedWords >= m_Data.CurrentLearnSession.Words.Count)
            {
                m_Data.CompletedSessions.Add(m_Data.CurrentLearnSession);
                m_Data.CurrentLearnSession = null;
            }

            if (!Directory.Exists(CacheFolderName))
                Directory.CreateDirectory(CacheFolderName);

            File.WriteAllText(CacheFileName, JsonConvert.SerializeObject(m_Data));
        }

        public Word GetWord(string _English)
        {
            return m_WordsCache[_English];
        }

        #region service methods

        void UpdateWordsCache()
        {
            m_WordsCache = new Dictionary<string, Word>();
            foreach (Word word in m_Data.Words)
                m_WordsCache[word.English] = word;
        }

        string GetJsonFromCache()
        {
            if (!Directory.Exists(CacheFolderName))
                Directory.CreateDirectory(CacheFolderName);

            return File.Exists(CacheFileName) ? File.ReadAllText(CacheFileName) : string.Empty;
        }

        

        #endregion
    }
}
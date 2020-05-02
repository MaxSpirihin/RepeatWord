using Android.App;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RepeatWord
{
    [Activity(Label = "StatisticsActivity")]
    public class StatisticsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_statistics);
            TextView textView = FindViewById<TextView>(Resource.Id.textViewStatistics);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format(
                "Words count : {0}", 
                WordsManager.Instance.Data.Words.Count
            ));

            List<RepeatSession> fullSessions = WordsManager.Instance.Data.CompletedSessions.Where(_S => _S.RepeatSessionType == RepeatSessionType.FULL_REPEAT).ToList();
            builder.AppendLine(GetSessionStat("Full sessions", RepeatSessionType.FULL_REPEAT));
            builder.AppendLine(GetSessionStat("Repeat forgotten", RepeatSessionType.REPEAT_FORGOTTEN));
            builder.AppendLine(GetSessionStat("Daily repeat", RepeatSessionType.DAILY_REPEAT));
            builder.AppendLine(GetSessionStat("Daily repeat random", RepeatSessionType.DAILY_REPEAT_RANDOM));
            
            textView.Text = builder.ToString();
        }

        string GetSessionStat(string _Text, RepeatSessionType _Type)
        {
            List<RepeatSession> sessions = WordsManager.Instance.Data.CompletedSessions.Where(_S => _S.RepeatSessionType == _Type).ToList();
            int allWords = sessions.Sum(_S => _S.Words.Count);
            int forgotten = sessions.Sum(_S => _S.Words.Count(_W => _W.IsForgotten));
            return string.Format(
                "\n{0} : {1}. Average forgotten: {2}% ({3}/{4})",
                _Text,
                sessions.Count,
                allWords > 0 ? forgotten * 100 / allWords : 0,
                forgotten,
                allWords
            );
        }
    }
}
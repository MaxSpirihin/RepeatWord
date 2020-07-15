using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepeatWord.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify sheet file name");
                return;
            }

            string fileName = args[0];
            string cacheFolder = Path.Combine(Path.GetDirectoryName(fileName), "RepeatWord");
            try
            {
                byte[] data = File.ReadAllBytes(fileName);
                string er;
                List<Word> words = WordsParser.ParseFromExcel(data, out er);
               
                if (words.Count > 0)
                {
                    WordsManager.Instance.Init(cacheFolder);
                    WordsManager.Instance.SetWords(words);
                    Console.WriteLine("Success parsing " + words.Count + " words");
                }
                else
                {
                    Console.WriteLine("Exception while parsing " + er);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

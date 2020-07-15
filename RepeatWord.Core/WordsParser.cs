using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace RepeatWord
{
    public static class WordsParser
    {
        public static List<Word> ParseFromExcel(byte[] _FileData, out string _Error)
        {
            try
            {
                List<Word> words = new List<Word>();
                using (MemoryStream stream = new MemoryStream(_FileData))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(stream))
                    {
                        //Get a WorkSheet by index. Note that EPPlus indexes are base 1, not base 0!
                        ExcelWorksheet firstWorksheet = excelPackage.Workbook.Worksheets[1];

                        for (int tableNum = 1; tableNum <= excelPackage.Workbook.Worksheets.Count; tableNum++)
                        {
                            ExcelWorksheet table = excelPackage.Workbook.Worksheets[tableNum];
                            int rows = GetRowsCount(table);
                            Console.WriteLine("Parse table " + table.Name + ". Rows = " + rows);
                            for (var i = 1; i <= rows; i++)
                            {
                                string en = table.Cells[i, 1].Value as string;
                                string ru = table.Cells[i, 2].Value as string;

                                if (!string.IsNullOrWhiteSpace(en) && !string.IsNullOrWhiteSpace(ru))
                                {
                                    words.Add(new Word()
                                    {
                                        English = en,
                                        Russian = ru,
                                        ListName = table.Name,
                                        ListNum = tableNum,
                                        Row = i
                                    });
                                }
                            }
                        }
                    }
                }

                _Error = "";
                return words;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR while parse excel " + e.Message);
                _Error = e.Message;
                return new List<Word>();
            }
        }

        public static List<Word> ParseFromGoogleSheetJson(string _Json, int _ListNum, out Exception _Error)
        {
            try
            {
                var data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_Json);
                var feed = (Dictionary<string, object>)data["feed"];
                var rowCountHolder = (Dictionary<string, object>)feed["gs$rowCount"];
                var colCountHolder = (Dictionary<string, object>)feed["gs$colCount"];
                var titleHolder    = (Dictionary<string, object>)feed["title"];
                string listName = (string)titleHolder["$t"];
                
                string[,] table = new string[Convert.ToInt32(rowCountHolder["$t"]), Convert.ToInt32(colCountHolder["$t"])];

                var entries = (List<object>)feed["entry"];

                int realRowCount = 1;
                foreach (Dictionary<string, object> entry in entries)
                {
                    var cell = (Dictionary<string, object>)entry["gs$cell"];
                    int row = Convert.ToInt32(cell["row"]);
                    int col = Convert.ToInt32(cell["col"]);
                    realRowCount = Math.Max(realRowCount, row);
                    table[row, col] = (string) cell["$t"];
                }
            
                List<Word> words = new List<Word>();
                for (var i = 1; i <= realRowCount; i++)
                {
                    string en = table[i, 1];
                    string ru = table[i, 2];

                    if (!string.IsNullOrWhiteSpace(en) && !string.IsNullOrWhiteSpace(ru))
                    {
                        words.Add(new Word()
                        {
                            English = en,
                            Russian = ru,
                            ListName = listName,
                            ListNum = _ListNum,
                            Row = i
                        });
                    }
                }

                _Error = null;
                return words;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR while parse _Json " + e.Message);
                _Error = e;
                return new List<Word>();
            }
        }

        static int GetRowsCount(ExcelWorksheet _Table)
        {
            //_Table.Rows.Count is working wrong. So check out by ourselves
            int emptyCount = 0;
            int rowCount = 1;
            int lastNotEmptyRow = 0;
            while (emptyCount < 10)
            {
                string en = _Table.Cells[rowCount, 1].Value as string;
                if (string.IsNullOrWhiteSpace(en))
                {
                    emptyCount++;
                }
                else
                {
                    lastNotEmptyRow = rowCount;
                    emptyCount = 0;
                }
                
                rowCount++;
            }

            return lastNotEmptyRow;
        }

    }
}
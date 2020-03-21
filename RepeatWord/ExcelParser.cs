using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using OfficeOpenXml;

namespace RepeatWord
{
    public static class ExcelParser
    {
        public static List<Word> Parse(byte[] _FileData)
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
                                        Row = i
                                    });
                                }
                            }
                        }
                    }
                }

                return words;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR while parse excel " + e.Message);
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
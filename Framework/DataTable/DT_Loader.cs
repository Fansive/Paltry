using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System.Linq;
using Paltry.Others;
using System.Text.RegularExpressions;

namespace Paltry
{
    public class DT_Loader 
    {
        public static void TryLoadDT()//外部接口
        {
            if (PaltryCongfig.SO.DT_Enable) 
            {
                DT_Loader loader = new();
                DT_CodeWriter codeGenerator = new();

                var list = loader.ReadAllWorkBooks();
                if (list != null)
                    codeGenerator.Write(list);
            }
        }
         
        bool hasModified = false;
        public List<(XSSFWorkbook workbook,string bookName)> ReadAllWorkBooks()
        {
            List<(XSSFWorkbook, string)> resultBooks = new();
            ReadAllWorkBooksCore(PaltryCongfig.SO.DT_ExcelRootDirectory, resultBooks);
            if (!hasModified)
                resultBooks = null;
            return resultBooks;

            void ReadAllWorkBooksCore(string directoryPath,List<(XSSFWorkbook,string)> books)
            {
                foreach (var directory in Directory.GetDirectories(directoryPath))
                    ReadAllWorkBooksCore(directory,books);

                foreach (var excel in 
                    from path in Directory.GetFiles(directoryPath, "*.xlsx")
                    let f = Path.GetFileName(path)
                    where !f.StartsWith("~$") && !f.EndsWith("_Duplicate.xlsx")
                    select path)
                {
                    if(File.GetLastWriteTime(excel) > lastDTGeneratedTime)
                    {   //若上次修改Excel的时间比上次产生DataTable的时间晚,说明要重新导入
                        hasModified = true;
                    }
                    string excel_copy = excel.Replace(".xlsx", "_Duplicate.xlsx");
                    File.Copy(excel, excel_copy, true);
                    books.Add((new XSSFWorkbook(excel_copy),Path.GetFileNameWithoutExtension(excel)));
                }
            }
        }

        private DateTime? _lastDTGeneratedTime;
        private DateTime? lastDTGeneratedTime
        {
            get
            {
                if (_lastDTGeneratedTime != null) 
                    return _lastDTGeneratedTime;
                //若并不存在DT代码,说明还未生成过,直接认为需要生成
                if (!File.Exists(DT_CodeWriter.DT_CodePath))
                    return _lastDTGeneratedTime = DateTime.MinValue;

                //从先前的DataTable中获取时间戳
                var lastDtCode = File.ReadAllText(DT_CodeWriter.DT_CodePath);
                Regex regex = new("Generate Time Stamp:(.+)using", RegexOptions.Singleline);
                var timeStamp = regex.Match(lastDtCode).Groups[1].Value;
                return _lastDTGeneratedTime = DateTime.Parse(timeStamp.Trim());
            }
        }
    }
}


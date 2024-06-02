using NPOI.SS.UserModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static System.Reflection.BindingFlags;
namespace System.Runtime.CompilerServices { class IsExternalInit { } }
//namespace Paltry
//{
//    public record DT_InstanceCreator(Dictionary<string, (ISheet sheet, string[] itemTypes)> CSVInfo)
//    {
//        private object dtObj;
//        string DTObjName = "DataTable";
//        /// <summary>
//        /// 产生数据表实例并将其序列化
//        /// </summary>
//        /// <param name="dtType">数据表最外层类的类型</param>
//        public void CreateInstance(Type dtType)
//        {
//            dtObj = Activator.CreateInstance(dtType);

//            foreach (var book in dtType.GetFields(Public | Instance))
//                book.SetValue(dtObj, CreateBook(book.FieldType));
            
//            dtType.GetField("instance", NonPublic | Static)
//                .SetValue(dtObj, dtObj);//将创建的数据表实例赋值给instance

//            JsonMgr.Save2StreamingAssets(dtObj, DTObjName);
//        }
//        private object CreateBook(Type bookType)
//        {
//            var bookObj = Activator.CreateInstance(bookType);
//            foreach (var sheet in bookType.GetFields())
//                sheet.SetValue(bookObj, CreateSheet(bookType,sheet.FieldType));
//            return bookObj;
//        }
//        private object CreateSheet(Type bookType,Type sheetType)
//        {
//            var sheetObj = Activator.CreateInstance(sheetType);
//            if (sheetType.Name.EndsWith("KVP"))//KVP直接new一个就行
//                return sheetObj;

//            //处理CSV
//            Type entryType = sheetType.GetGenericArguments()[0];
//            var fields = entryType.GetFields();
//            var (sheet, itemTypes) = CSVInfo[bookType.Name+entryType.Name];
//            //CSVSheet有三行是非数据行,所以减3,而LastRowNum是索引,+1,最终减2
//            Array entries = Array.CreateInstance(entryType, sheet.LastRowNum - 2);
//            int entryIndex = 0;//给entries赋值所用的索引

//            foreach (IRow row in sheet)//遍历CSV中的每一行,每一行创建一个entry
//            {
//                if (!int.TryParse(row.GetCellStr(0).AsSpan()[1..^0], out var _))
//                    continue;//只有 #3 这种数字结尾的行才是数据行
//                var entry = Activator.CreateInstance(entryType);
//                //为entry的各项字段赋值 
//                for (int i = 0; i < fields.Length; i++)
//                {
//                    fields[i].SetValue(entry,//itemTypes是entry里各个字段的类型名(用于DT_Parser)
//                        DT_Parser.BuildData(row.GetCellStr(i+1), itemTypes[i]));
//                }
//                entries.SetValue(entry, entryIndex++);
//            }
//            sheetType.GetField("Entries").SetValue(sheetObj, entries);
//            return sheetObj;
//        }

//        /// <summary>
//        /// 将每一张表转换为表格字符串,可打印在控制台中,用来确认读取的数据是否正确
//        /// </summary>
//        /// <returns>包含每一张表字符串的List</returns>
//        public List<string> GetDataTableDisplay()
//        {
//            List<string> res = new();
//            Type dtType = dtObj.GetType();
//            foreach (var book in dtType.GetFields(NonPublic | Instance))
//            {
//                foreach (var sheet in book.FieldType.GetFields())
//                {
//                    var sheetObj = sheet.GetValue(book.GetValue(dtObj));
//                    var fields = sheet.FieldType.GetFields();
//                    StringBuilder[] sheetRows;
//                    StringBuilder sheetText = new(book.FieldType.Name);

//                    if (sheet.FieldType.Name.EndsWith("KVP"))
//                    {
//                        sheetRows = new StringBuilder[fields.Length+1];//字段行加上#变量行
//                        sheetRows[0] = new("#变量");
//                        for (int i = 1; i < sheetRows.Length; i++)//#列
//                            sheetRows[i] = new(fields[i - 1].Name);
//                        RightAlign(sheetRows);
//                        for (int i = 1; i < sheetRows.Length; i++)//数据列
//                            sheetRows[i].Append(ToStr(fields[i - 1].GetValue(sheetObj)));
//                        RightAlign(sheetRows);
//                        sheetText.Append(".").Append(sheet.Name).AppendLine("(KVP)");
//                    }
//                    else//CSV
//                    {   //这里fields只有一个field,即T[] Entries,是真正的数据源
//                        var entriesObj = fields[0].GetValue(sheetObj) as Array;
//                        sheetRows = new StringBuilder[entriesObj.Length+1];
//                        sheetRows[0] = new("#变量");
//                        for (int i = 1; i < sheetRows.Length; i++)
//                            sheetRows[i] = new("#" + i);//单独处理第一列的#行
//                        RightAlign(sheetRows);

//                        var entryFields = sheet.FieldType.GetGenericArguments()[0].GetFields();
//                        foreach (var field in entryFields)
//                        {
//                            sheetRows[0].Append(field.Name);
//                            for (int i = 1; i < sheetRows.Length; i++)//取出entries每一项的field数据
//                                sheetRows[i].Append(ToStr(field.GetValue(entriesObj.GetValue(i-1))));
//                            RightAlign(sheetRows);
//                        }
//                        sheetText.Append(".").Append(sheet.Name).AppendLine("(CSV)");
//                    }

//                    foreach (var row in sheetRows)
//                        sheetText.Append(row).Append('\n');
//                    res.Add(sheetText.ToString());
//                }

//            }
//            return res;


//            void RightAlign(StringBuilder[] sheetText)
//            {//使得每一列元素右对齐,列间距为2
//                int max = sheetText.Max(x => x.Length);
//                foreach (var i in sheetText)
//                    i.Append(new string(' ', max - i.Length + 2));
//            }
//            StringBuilder ToStr(object value)//主要是将表格数据里的数组展开为字符串
//            {
//                StringBuilder res = new();
//                if (value is Array arr)
//                {
//                    res.Append('[');
//                    for (int i = 0; i < arr.Length; i++)
//                        res.Append(ToStr(arr.GetValue(i))).Append(',');
//                    res.Append(']');
//                }
//                else
//                    res.Append(value.ToString());

//                return res;
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Paltry
{
    public class DT_Parser
    {
        /// <summary>
        /// 保存所有可解析类型(不包括数组)的信息,用于构建数据字面量和实际值
        /// <para>该Dict的key为primitive类型名(不含数组),Bracket1/2是仅带括号类型才有的</para>
        /// <para>如需翻译转义字符(如\"),也应写在LiteralBuilder和DataBuilder里</para>
        /// </summary>
        //private static Dictionary<string,
        //    (char? Bracket1, char? Bracket2,
        //    Func<string, string> LiteralBuilder,
        //    Func<string, object> DataBuilder)> typeInfos = new()
        //    {
        //        {"int",(null,null,x=>x,x=>int.Parse(x)) },
        //        {"float",(null,null,x=>x+'f',x=>float.Parse(x)) },
        //        {"string",('\"','\"',x=>InterpretEscape_Literal(x),x=>InterpretEscape_Data(x)) },
        //        {"bool",(null,null,x=>x.ToLower(),x=>bool.Parse(x)) },
        //        {"char",(null,null,x=>x,x=>x.Trim('\'')) },
        //        {"Vector2",('(',')',x=>"new Vector2"+x.Replace(",", "f,").Replace(")", "f)")
        //            ,x =>{
        //                int mid = x.IndexOf(',');
        //                return new Vector2(float.Parse(x[1..mid]),float.Parse(x[(mid+1)..^1]));
        //            })},
        //        {"Vector3",('(',')',x=>"new Vector3"+x.Replace(",", "f,").Replace(")", "f)")
        //            ,x =>{
        //                int c1 = x.IndexOf(',');
        //                int c2 = x.LastIndexOf(",");
        //                return new Vector3(float.Parse(x[1..c1]),
        //                    float.Parse(x[(c1+1)..c2]),float.Parse(x[(c2+1)..^1]));
        //            })},
        //    };
        static DT_Parser()
        {
            Custom_DT_Parser.RegisterTypes(); 
        }

        private static Dictionary<string,
            (char? Bracket1, char? Bracket2,Func<string, string> LiteralBuilder)>
            typeInfos = new()
            {
                {"int",(null,null,x=>x)},
                {"float",(null,null,x=>x+'f')},
                {"string",('\"','\"',x=>InterpretEscape_Literal(x))},
                {"bool",(null,null,x=>x.ToLower())},
                {"char",(null,null,x=>x)},
                {"Vector2",('(',')',x=>"new Vector2"+x.Replace(",", "f,").Replace(")", "f)"))},
                {"Vector3",('(',')',x=>"new Vector3"+x.Replace(",", "f,").Replace(")", "f)"))},
                {"Quaternion",('(',')',x=>"new Quaternion"+x.Replace(",", "f,").Replace(")", "f)"))},
            };

        #region TryParseType 仅用于推断省略的默认类型
        public static bool TryParseType(string data,out string type)
        {
            if (TryParseType_Primitive(data, out type)) return true;
            else if (TryParseType_Array(data,out type)) return true;

            return false; 
        }
        private static bool TryParseType_Primitive(string data,out string type)
        { 
            type = null;
            if (int.TryParse(data, out int _)) type = "int";
            else if (float.TryParse(data, out float _)) type = "float";
            else if (bool.TryParse(data, out bool _)) type = "bool";
            else if (data.StartsWith("\"")) type = "string";
            else if(data.StartsWith('\'')) type = "char";
            else if (data.StartsWith('('))
            {
                type = (1 + data.Count(c => c == ',')) switch
                {
                    2 => "Vector2",
                    3 => "Vector3",
                    4 => "Quaternion",
                    _ => throw new FormatException(StrUtil.LogCyan(
                            $"<DataTable>尝试从 {data} 中推断类型失败:逗号数目错误"))
                };
            }
            else 
                return false;

            return true;
        }
        private static bool TryParseType_Array(string data, out string type)
        {
            type = null;
            var span = data.AsSpan().Trim();
            if(!IsArrayData(span))//并不是[]包围,说明语法不对
                return false;

            //去掉最外层的[]
            span = span.Slice(1, span.Length - 2);
            span.Trim();

            int arrayRank = 1;
            bool isString = false;
            int vectorDimension = 1;
            for (int i = 0; i < span.Length; i++)
            {
                char c = span[i];

                if (c == '[') arrayRank = 2;
                else if (c == ']') break;
                else if (c == '\"')
                {
                    isString = true;
                    break;
                }
                else if (c == '(')
                {
                    int j = i + 1;
                    while (span[j++] != ')' && j < span.Length)
                    {
                        if (span[j] == ',') vectorDimension++;
                    }
                    break;
                }
            }

            string arrayType = arrayRank == 2 ? "[][]" : "[]";
            if (isString)
                type = "string" + arrayType;
            else if (vectorDimension != 1)
                type = "Vector" + vectorDimension + arrayType;
            else
            {
                if(arrayRank == 2)
                    span = span[1..span.IndexOf(']')].Trim();

                //分析数组里的原始类型是什么
                int elemEnd = span.IndexOf(',') == -1
                    ? span.Length : span.IndexOf(',');
                if (TryParseType_Primitive(span[0..elemEnd].Trim().ToString(), out type))
                    type += arrayType;
                else//无法推断原始类型,说明语法不合法
                    return false;
            }


            return true;
        }
        #endregion

        #region BuildLiteral 将表格里的字符串数据转换为代码里的初始化字面量(或new 表达式)
        /// <summary>
        /// 将表格里的字符串数据转换为代码里的初始化字面量(或new 表达式)
        /// </summary>
        public static string BuildLiteral(string data,string type) 
        {
            if (data == null) return "default";
            return type.Count(c => c == '[') switch//根据'['数量判断是不是数组,如果是,是几维的
            {
                0 => BuildLiteral_Primitive(data, type),
                1 => BuildLiteral_Array(data, type, 1),
                2 => BuildLiteral_Array(data, type, 2),
                _ => throw new ArgumentException(StrUtil.LogCyan("<DataTable>不支持三维或更高维数组"))
            };
        }
        private static string BuildLiteral_Array(string data,string type,in int arrayRank)
        {
            StringBuilder dataSB = new("new[]{");
            var trimedData = data.AsSpan().Trim();
            if (!IsArrayData(trimedData))
                throw new FormatException(StrUtil.LogCyan($"<DataTable> {data} 不是数组"));
            var values = SplitArrayValues(trimedData[1..^1].ToString(), GetInnerType(type));

            foreach (var value in values)
            {
                if(arrayRank == 1)
                {
                    var literal = BuildLiteral_Primitive(value, GetInnerType(type));
                    dataSB.Append(literal).Append(',');
                }
                else if(arrayRank == 2)
                {
                    var literal = BuildLiteral_Array(value, GetInnerType(type), 1);
                    dataSB.Append(literal).Append(',');
                }
            }

            dataSB.Append('}');
            return dataSB.ToString();
        }
        private static string BuildLiteral_Primitive(string data,string primitiveType)
        {
            if (typeInfos.TryGetValue(primitiveType, out var info))
                return info.LiteralBuilder(data);
            else throw new KeyNotFoundException(StrUtil.LogCyan($"<DataTable>类型<{primitiveType}>未注册或表格中未指明类型"));
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 切分数组里的元素,并以字符串形式返回
        /// </summary>
        /// <para><paramref name="data"/> 若为括号包起来的元素,其中出现自身括号(如string的"")或"[]"时,必须用'\'转义</para>
        /// <param name="data">数组元素字符串,如[2,3,4],但不包括数组本身的"[]"(应为2,3,4),调用时需Slice掉两两头的[]</param>
        /// <param name="innerType">数组元素类型,如int[]=>int,int[][]=>int[],需为默认类型或已注册到<see cref="typeInfos"/></param>
        /// <exception cref="KeyNotFoundException">需确保该类型已注册过</exception>
        private static List<string> SplitArrayValues(string data,string innerType)
        {
            if (innerType.Contains('['))//数组元素的类型也是数组,即int[][]=>int[]
                return SplitCore_ByBracket(data, '[', ']');
            else if(typeInfos.TryGetValue(innerType,out var info))
                return info.Bracket1 == null ? SplitCore_ByComma(data)//该类型不含括号,直接按','切分
                    : SplitCore_ByBracket(data, info.Bracket1, info.Bracket2);
            else 
                throw new KeyNotFoundException(StrUtil.LogCyan($"<DataTable>类型<{innerType}>尚未注册")); 



            List<string> SplitCore_ByComma(string data)
                => new(data.Split(',').Where(v => v != "").Select(v => v.Trim()));

            List<string> SplitCore_ByBracket(string data, char? bracket1, char? bracket2)
            {
                List<string> values = new();
                int left = -1;
                for (int i = 0; i < data.Length; i++)
                {
                    char c = data[i];
                    if (i != 0 && data[i - 1] == '\\' && //如果是转义的括号就跳过
                        (c == bracket1 || c == bracket2 || c is '[' or ']'))
                        continue;

                    if(bracket1 != bracket2)//用于(),[],{}的情况
                    {
                        if (c == bracket1) left = i;
                        else if (c == bracket2)
                        {
                            values.Add(data[left..(i + 1)]);
                            left = -1;
                        }
                    }
                    else//用于"",''的情况
                    {
                        if (c == bracket1 && left == -1) left = i;
                        else if(c == bracket1 && left != -1)
                        {
                            values.Add(data[left..(i + 1)]);
                            left = -1;
                        }
                    }
                    
                }
                return values;
            }

        }
         
        /// <summary>
        /// 获取数组元素的类型,例如:int[]=>int,bool[][]=>bool[]
        /// </summary>
        /// <param name="type">数组类型,如int[]</param>
        /// <param name="arrayRank">数组的维度数</param>
        /// <returns>数组脱去一层后的类型</returns>
        private static string GetInnerType(string type)
        {
            if (type.Contains('['))//有'[',脱一层方括号
                return type[0..^2];
            else//没有,说明已经是原始类型了
                return type;
        }
        /// <summary>
        /// 获取给定类型的原始类型,无论嵌套了多少层,例如以下输入均返回int:int[],int,int[][]
        /// </summary>
        /// <param name="type">任意类型</param>
        /// <returns>原始类型</returns>
        private static string GetPrimitiveType(string type)
        {
            int end = type.IndexOf('[');
            if (end == -1)//没有'[',已经是原始类型,直接返回
                return type;
            else
                return type[0..end];
        }
        private static bool IsArrayData(ReadOnlySpan<char> data)
        {
            return data.StartsWith("[") && data.EndsWith("]");
        }

        /// <summary>
        /// 解释excel里数据的转义字符,例如:  \" => " 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string InterpretEscape_Literal(string str)//Escape:转义(字符)
        {
            return "@" + new StringBuilder(str)
                .Replace(@"\""", "\"\"").Replace(@"\[", "[").Replace(@"\]", "]")
                .Replace(@"\\", @"\").ToString();
        } 
        private static string InterpretEscape_Data(string str)//Escape:转义(字符)
        {
            return new StringBuilder(str)
                .Replace(@"\""", "\"\"").Replace(@"\[", "[").Replace(@"\]", "]")
                .Replace(@"\\", @"\").ToString().Trim();
        }
        #endregion

        #region 注册接口
        /// <summary>
        /// 注册自定义类型,该方法仅需在<see cref="Custom_DT_Parser.RegisterTypes"/>中调用
        /// </summary>
        /// <param name="type">可直接写在代码里的类型名,若位于某命名空间中,需包含命名空间一起(或在DT配置里添加using),例如System.Text.StringBuilder</param>
        /// <param name="literalBuilder">字面量构建方法,要求将表格里的数据字符串转换为代码里的字面量</param>
        /// <param name="bracket1">若该类型需用括号表示(如Vector2的(),string的""),该参数传入左括号,否则则为null</param>
        /// <param name="bracket2">若该类型需用括号表示(如Vector2的(),string的""),该参数传入右括号,否则则为null</param>
        public static void RegisterCustomType(string type,Func<string,string> literalBuilder,char? bracket1=null,char? bracket2=null)
        {
            typeInfos.Add(type, (bracket1, bracket2, literalBuilder));
        }
        /// <summary>
        /// 注册自定义枚举,是<see cref="RegisterCustomType"/>针对枚举类型的简化方法
        /// 在表格里填写的枚举常量,直接写枚举成员即可,不需要加上[枚举名.]
        /// 示例:<![CDATA[
        /// RegisterCustomEnumType("MyNamespace.MyEnum");]]>
        /// </summary>
        /// <param name="enumType"></param>
        public static void RegisterCustomEnumType(string enumType)
        {
            RegisterCustomType(enumType, x => enumType + "." + x);
        }
        #endregion


        //public static object BuildData(string data,string type)
        //{
        //    return type.Count(c => c == '[') switch//根据'['数量判断是不是数组,如果是,是几维的
        //    {
        //        0 => BuildData_Primitive(data, type),
        //        1 => BuildData_Array(data, type, 1),
        //        2 => BuildData_Array(data, type, 2),
        //        _ => throw new ArgumentException("不支持三维或更高维数组")
        //    };

        //    object BuildData_Array(string data,string type, int arrayRank)
        //    {
        //        var values = SplitArrayValues(data.AsSpan().Trim()[1..^1].ToString(), GetInnerType(type));
        //        List<object> list = null;

        //        if(arrayRank == 1)
        //            list = values.ConvertAll(v=>BuildData_Primitive(v, GetPrimitiveType(type)));
        //        else if(arrayRank == 2)
        //            list = values.ConvertAll(v=>BuildData_Array(v, GetInnerType(type),1));

        //        Array array = Array.CreateInstance(list[0].GetType(), list.Count);
        //        for (int i = 0; i < list.Count; i++)
        //            array.SetValue(list[i], i);
        //        return array;
        //    }
        //    object BuildData_Primitive(string data, string primitiveType) =>
        //        typeInfos[primitiveType].DataBuilder(data);
        //}
    }
}

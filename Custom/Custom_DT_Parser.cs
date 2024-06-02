using UnityEngine;
using Paltry;
public class Custom_DT_Parser  
{
    /// <summary>
    /// 在此处通过<see cref="DT_Parser.RegisterCustomType"/>为数据表模块添加自定义类型
    /// 若添加枚举,有更简便的方法:<see cref="DT_Parser.RegisterCustomEnumType(string)"/>
    /// 以下是一个示例: <![CDATA[
    /// DT_Parser.RegisterCustomType("Vector4", 
    /// x => "new Color" + x.Replace(",", "f,").Replace(")", "f)"),'(',')');
    /// //添加Color支持,将表格里的 (1,4.2,5,7) 转换为字面量 new Color(1f,4.2f,5f,7f)
    /// ]]>
    /// <para>自定义类型有一定限制,具体可参考文档</para>
    /// </summary>
    public static void RegisterTypes()
    {

    }
}

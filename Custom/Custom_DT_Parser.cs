using UnityEngine;
using Paltry;
public class Custom_DT_Parser  
{
    /// <summary>
    /// �ڴ˴�ͨ��<see cref="DT_Parser.RegisterCustomType"/>Ϊ���ݱ�ģ������Զ�������
    /// �����ö��,�и����ķ���:<see cref="DT_Parser.RegisterCustomEnumType(string)"/>
    /// ������һ��ʾ��: <![CDATA[
    /// DT_Parser.RegisterCustomType("Vector4", 
    /// x => "new Color" + x.Replace(",", "f,").Replace(")", "f)"),'(',')');
    /// //���Color֧��,�������� (1,4.2,5,7) ת��Ϊ������ new Color(1f,4.2f,5f,7f)
    /// ]]>
    /// <para>�Զ���������һ������,����ɲο��ĵ�</para>
    /// </summary>
    public static void RegisterTypes()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Paltry
{
    /*�ڴ˴�����Paltry��һЩ��������*/

    /// <summary>
    /// �ڸ�ö���¼�����Ҫ��Ϊ��������������
    /// ����GameObject,�������͵�ֱ����Pool<T>����
    /// </summary>
    public enum GOType
    {
        bullet
    }
    public struct UIName
    {
        public const string LoginPanel = nameof(LoginPanel);
        public const string SelectPanel = nameof(SelectPanel);
    }
    public partial struct EventName
    {
        //�¼����ַ���������������(����ַ���������ʾ,����ɾ��)
        public const string test = nameof(test);

    }
    public class PaltryConst
    {
        /// <summary>
        /// AB��/AB����Դ����ʱ,���ȸ��¼��
        /// </summary>
        public static YieldInstruction LoadInfoUpdateInternal = null;


    }

}

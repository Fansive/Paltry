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

    }
    public struct UIName
    {
        public const string MainMenuPanel = nameof(MainMenuPanel);
        public const string SettingPanel = nameof(SettingPanel);
        public const string GameSelectPanel = nameof(GameSelectPanel);
        public const string LoadingPanel = nameof(LoadingPanel);
        public const string TetrisPanel = nameof(TetrisPanel);
        public const string PausePanel = nameof(PausePanel);
        public const string TetrisEndPanel = nameof(TetrisEndPanel);
    }
    public partial struct EventName
    {
        //�¼����ַ���������������(����ַ���������ʾ,����ɾ��)
        public const string test = nameof(test);
        public const string ClearLine = nameof(ClearLine);

    }
    public class PaltryConst
    {
        /// <summary>
        /// AB��/AB����Դ����ʱ,���ȸ��¼��
        /// </summary>
        public static YieldInstruction LoadInfoUpdateInternal = null;


    }

}

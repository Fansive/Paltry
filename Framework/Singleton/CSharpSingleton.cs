namespace Paltry
{
    /// <summary>
    /// C#����ģ��,���ڲ��̳�Mono�ĵ���
    /// <para>#if UNITY_EDITOR��������ȷ������������ʱ���³�ʼ������</para>
    /// </summary>
    public abstract class CSharpSingleton<T> where T : class,new()
    {
        private static T instance;
        public static T Instance
        {
            protected set { instance = value; }
            get
            {
                if (instance == null)
                {
                    instance = new T();//��һ�η��ʵ���ʱ����ù��캯������ʼ��
#if UNITY_EDITOR                       //����,������ĳ�ʼ��д�ڹ��캯����ͺ�
                    MonoAgent._Instances_To_Reset += () => instance = null;//�������������ʹ����������ǰ��״̬
#endif
                }                      
                return instance;       
                                       
            }
        }
    }
}


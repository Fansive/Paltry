namespace Paltry
{
    /// <summary>
    /// C#单例模板,用于不继承Mono的单例
    /// <para>#if UNITY_EDITOR的内容是确保禁用域重载时重新初始化单例</para>
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
                    instance = new T();//第一次访问单例时会调用构造函数来初始化
#if UNITY_EDITOR                       //所以,单例类的初始化写在构造函数里就好
                    MonoAgent._Instances_To_Reset += () => instance = null;//避免禁用域重载使单例保持以前的状态
#endif
                }                      
                return instance;       
                                       
            }
        }
    }
}


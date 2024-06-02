using UnityEngine;

namespace Paltry
{
    /// <summary>
    /// Mono单例模板,在Awake里初始化,可以覆写Awake但一定要调用base.Awake()
    /// <para>不应在Awake里访问其他Mono单例,因为初始化顺序不确定,应该在Start中进行</para>
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    Debug.LogError($"单例<{typeof(T)}>尚未挂载在GameObject上或该单例覆写了Awake但未调用base.Awake()");
                }
                return instance;
            }
        }
        /// <summary>
        /// 覆写时请先加上base.Awake();
        /// </summary>
        protected virtual void Awake()
        {
            if(instance != this && instance != null)
            {
                Debug.LogError($"请不要重复创建单例<{typeof(T)}>", gameObject);
            }
            instance = this as T;
            //DontDestroyOnLoad(gameObject);是否在切场景时不销毁,由子类自己决定
        }
    }
}


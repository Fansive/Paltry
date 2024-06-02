using UnityEngine;

namespace Paltry
{
    /// <summary>
    /// Mono����ģ��,��Awake���ʼ��,���Ը�дAwake��һ��Ҫ����base.Awake()
    /// <para>��Ӧ��Awake���������Mono����,��Ϊ��ʼ��˳��ȷ��,Ӧ����Start�н���</para>
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
                    Debug.LogError($"����<{typeof(T)}>��δ������GameObject�ϻ�õ�����д��Awake��δ����base.Awake()");
                }
                return instance;
            }
        }
        /// <summary>
        /// ��дʱ���ȼ���base.Awake();
        /// </summary>
        protected virtual void Awake()
        {
            if(instance != this && instance != null)
            {
                Debug.LogError($"�벻Ҫ�ظ���������<{typeof(T)}>", gameObject);
            }
            instance = this as T;
            //DontDestroyOnLoad(gameObject);�Ƿ����г���ʱ������,�������Լ�����
        }
    }
}


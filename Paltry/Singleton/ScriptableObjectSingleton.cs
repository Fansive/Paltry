using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Paltry
{
    /// <summary>
    /// ��SO������,ʹ���ʸ��ӱ��
    /// </summary>
    public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private const string LoadPath = "MySoPath";
        private static T instance;
        public static T Instance
        {
            get
            {
                if(instance == null)
                {//�ڴ˴�������Դ���صķ�ʽ��·��,�̳иõ�����SOҪȷ��������SO�ļ���һ��
                    //instance = Resources.Load<T>(LoadPath+nameof(T));
                }
                return instance;
            }
        }
    }
}


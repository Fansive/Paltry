using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Paltry
{
    /// <summary>
    /// 将SO单例化,使访问更加便捷
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
                {//在此处填入资源加载的方式和路径,继承该单例的SO要确保类名和SO文件名一致
                    //instance = Resources.Load<T>(LoadPath+nameof(T));
                }
                return instance;
            }
        }
    }
}


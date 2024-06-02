using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Paltry
{
    /// <summary>
    /// 将SO单例化
    /// 有两种访问方式:在编辑器中和在运行时
    /// 若选择了在编辑器中,必须override <see cref="ScriptableObjectSingleton{T}.LoadPath"/>
    /// 不需要手动创建SO,如果没有,运行后会自动创建
    /// 若选择了在运行时,需要把SO手动打包到Addressable里
    /// 也可以两种访问方式都选择
    /// 使用流程:
    /// 1.新建脚本,继承自该类,写好所需的属性并提供LoadPath和LoadWay
    /// 2.编译运行
    /// 3.运行后SO就创建好了,可以往里面填数据了,并且以后不用再管
    /// PS:若SO类改名,则会新创建SO,原来的可手动删除
    /// </summary>
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T>
    {
        /// <summary>
        /// 对于在编辑器里加载的资源,指定加载路径目录(直接复制Unity里的路径即可)
        /// </summary>
        protected virtual string LoadPath { get; }
        protected abstract SoLoadWay LoadWay { get; }

        private string soName =>'/' + typeof(T).Name + ".asset";

        private static T so;
        public static T SO
        {
            get
            {
                if (so == null)
                { 
                    so = ScriptableObject.CreateInstance<T>();
                    if((so.LoadWay & SoLoadWay.Editor) != 0)
                    {
#if UNITY_EDITOR
                        var asset = AssetDatabase.LoadAssetAtPath<T>(so.LoadPath+so.soName);
                        if (asset == null)
                        {
                            AssetDatabase.CreateAsset(so,so.LoadPath+so.soName);
                            AssetDatabase.SaveAssets();
                        }
                        else so = asset;
#endif
                    }
                    else if((so.LoadWay & SoLoadWay.Runtime) != 0)
                    {
                        so = AALoader.Instance.LoadAsset<T>(typeof(T).Name);
                    }
                }
                
                return so;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在游戏初始化时,找到所有继承自<see cref="ScriptableObjectSingleton{T}"/>的子类
        /// 并调用它们的get访问器刷新SO数据
        /// 该方法仅在编辑器下会在启动时自动调用
        /// </summary>
        public static void RefreshSO()
        {
            foreach (var so in from t in ReflectionUtil.CurTypes
                               where t.BaseType?.IsGenericType == true
                               where t.BaseType.GetGenericTypeDefinition() ==
                                   typeof(ScriptableObjectSingleton<>)
                               select t)
            {
                _ = so.BaseType.GetProperty("SO").GetValue(null);
            }
        }
    }
#endif
    [Flags]
    public enum SoLoadWay
    {
        Editor = 1<<0,
        Runtime = 1<<1,
        EditorAndRuntime = Editor | Runtime,
    }
}


#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Paltry
{
    /// <summary>
    /// 调试工具,方便一些特定情况的调试
    /// 引用了UnieyEditor,发布前记得把引用该类的地方删掉
    /// </summary>
    public class DebugTool:CSharpSingleton<DebugTool>
    {
        int stopCount = 0;
        object sceneHierarchy;
        MethodInfo methodInfo;
        GameObject debugRoot;
        /// <summary>
        /// 在第time次执行到这里时,返回true,该方法只能同时在一处使用,所以用完记得删:)
        /// </summary>
        /// <param name="time">第几次执行到这里,从1开始计数</param>
        /// <returns></returns>
        public static bool StopAt(int time)
        {
            Instance.stopCount++;
            if(Instance.stopCount == time)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 在指定GameObject身上依附一个Text实时显示调试信息
        /// <para>不需要重写ToString</para>
        /// </summary>
        /// <param name="GetInfo">返回调试信息的方法</param>
        /// <param name="offset">文本位置偏移量,默认位置和GameObject一致</param>
        /// <param name="sizeDelta">文本框大小</param>
        public static void AttachText(GameObject sourceObj,Func<GameObject,string>GetInfo, Vector2 offset = default(Vector2), float sizeDelta = 1)
        {
            var tmp = Instance.AttachTextCore(sizeDelta);
            MonoAgent.Instance.AddUpdate(() =>
            {
                tmp.text = GetInfo(sourceObj);
                tmp.transform.position = (Vector2)sourceObj.transform.position + offset;
            });
        }

        /// <summary>
        /// 在Mono挂载的GameObject身上依附一个Text,实时显示调试信息
        /// <para>请重写继承Mono脚本的ToString以显示想要的信息</para>
        /// </summary>
        /// <param name="offset">文本位置偏移量,默认位置和GameObject一致</param>
        /// <param name="sizeDelta">文本框大小</param>
        public static void AttachText(MonoBehaviour sourceObj,Vector2 offset=default(Vector2),float sizeDelta =1)
        {
            var tmp = Instance.AttachTextCore(sizeDelta);
            MonoAgent.Instance.AddUpdate(() =>
            {
                tmp.text = sourceObj.ToString();
                tmp.transform.position = (Vector2)sourceObj.transform.position + offset;
            });
        }
        public TextMeshPro AttachTextCore(float sizeDelta)
        {
            if (debugRoot == null)
                debugRoot = new GameObject("DebugRoot");

            GameObject obj = new GameObject("AttachedDebugText");
            obj.transform.SetParent(debugRoot.transform);

            var tmp = obj.AddComponent<TextMeshPro>();
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0;
            tmp.fontSizeMax = 72;
            RectTransform rectTransform = tmp.transform as RectTransform;
            rectTransform.sizeDelta = Vector2.one * sizeDelta;

            return tmp;
        }
        #region SetExpand
        /// <summary>
        /// 将Hierarchy中的GameObject展开层级(如果有子对象)
        /// </summary>
        public static void SetExpand(string gameObjectName, bool useExpand = true)
        {
            Instance.SetExpandCore(GameObject.Find(gameObjectName),1, useExpand);
        }
        public static void SetExpand(GameObject go, bool useExpand = true)
        {
            Instance.SetExpandCore(go,1, useExpand);
        }
        /// <summary>
        /// 将Hierarchy中的GameObject展开层级(如果有子对象)
        /// <para>layer表示要展开几层</para>
        /// </summary>
        public static void SetExpand(string gameObjectName, int layer, bool useExpand = true)
        {
            Instance.SetExpandCore(GameObject.Find(gameObjectName), layer, useExpand);
        }
        public static void SetExpand(GameObject go,int layer,bool useExpand = true)
        {
            Instance.SetExpandCore(go, layer, useExpand);
        }
        private void SetExpandCore(GameObject go,int layer,bool useExpand)
        {
            var layerQueue = new Queue<GameObject>();
            var layerQueue2 = new Queue<GameObject>();
            layerQueue.Enqueue(go);
            while(layer-- != 0)
            {
                while (layerQueue.Count != 0)
                {
                    var obj = layerQueue.Dequeue();
                    SetExpandSingleCore(obj, useExpand);
                    for (int i = 0; i < obj.transform.childCount; i++)
                        layerQueue2.Enqueue(obj.transform.GetChild(i).gameObject);
                }
                if (layer-- == 0)
                    return;
                while (layerQueue2.Count != 0)
                {
                    var obj = layerQueue2.Dequeue();
                    SetExpandSingleCore(obj, useExpand);
                    for (int i = 0; i < obj.transform.childCount; i++)
                        layerQueue.Enqueue(obj.transform.GetChild(i).gameObject);
                }
            }
        }
        private void SetExpandSingleCore(GameObject go, bool useExpand)
        {
            InitExpand();
            methodInfo.Invoke(sceneHierarchy, new object[] { go.GetInstanceID(), useExpand });
        }
        private void InitExpand()
        {
            sceneHierarchy = sceneHierarchy ?? GetSceneHierarchy();
            methodInfo = methodInfo ?? 
                sceneHierarchy
                .GetType()
                .GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);
        }
        private object GetSceneHierarchy()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorWindow window = EditorWindow.focusedWindow;

            return typeof(EditorWindow).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow")
                .GetProperty("sceneHierarchy")
                .GetValue(window);
        }
        #endregion
    }
}
#endif
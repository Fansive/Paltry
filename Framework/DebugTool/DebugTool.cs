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
    /// ���Թ���,����һЩ�ض�����ĵ���
    /// ������UnieyEditor,����ǰ�ǵð����ø���ĵط�ɾ��
    /// </summary>
    public class DebugTool:CSharpSingleton<DebugTool>
    {
        int stopCount = 0;
        object sceneHierarchy;
        MethodInfo methodInfo;
        GameObject debugRoot;
        /// <summary>
        /// �ڵ�time��ִ�е�����ʱ,����true,�÷���ֻ��ͬʱ��һ��ʹ��,��������ǵ�ɾ:)
        /// </summary>
        /// <param name="time">�ڼ���ִ�е�����,��1��ʼ����</param>
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
        /// ��ָ��GameObject��������һ��Textʵʱ��ʾ������Ϣ
        /// <para>����Ҫ��дToString</para>
        /// </summary>
        /// <param name="GetInfo">���ص�����Ϣ�ķ���</param>
        /// <param name="offset">�ı�λ��ƫ����,Ĭ��λ�ú�GameObjectһ��</param>
        /// <param name="sizeDelta">�ı����С</param>
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
        /// ��Mono���ص�GameObject��������һ��Text,ʵʱ��ʾ������Ϣ
        /// <para>����д�̳�Mono�ű���ToString����ʾ��Ҫ����Ϣ</para>
        /// </summary>
        /// <param name="offset">�ı�λ��ƫ����,Ĭ��λ�ú�GameObjectһ��</param>
        /// <param name="sizeDelta">�ı����С</param>
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
        /// ��Hierarchy�е�GameObjectչ���㼶(������Ӷ���)
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
        /// ��Hierarchy�е�GameObjectչ���㼶(������Ӷ���)
        /// <para>layer��ʾҪչ������</para>
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
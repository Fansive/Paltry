using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Paltry.Extension
{
    public static class CommonExtension
    {

        /// <summary>
        /// 给LineRenderer添加点,会自动处理PositionCount
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="position"></param>
        public static void AddPosition(this LineRenderer lr,Vector3 position)
        {
            lr.SetPosition(++lr.positionCount-1, position);
        }
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }
        public static void SetParent(this GameObject go,GameObject parent)
        {
            go.transform.SetParent(parent.transform);
        }
        public static void SetParent(this GameObject go,Transform parent)
        {
            go.transform.SetParent(parent);
        }
        public static void SetPosition(this GameObject go,Vector2 position)
        {
            go.transform.position = position;
        }
        public static void SetLocalPosition(this GameObject go,Vector2 position)
        {
            go.transform.localPosition = position;
        }
        /// <summary>
        /// 通过给定路径查找子对象,路径格式是:"第一层/第二层/第三层"(和文件路径一样)
        /// </summary>
        public static Transform FindByPath(this Transform parent, string path)
        {
            foreach(string subPath in path.Split('/'))
                parent = parent.Find(subPath);
            return parent;
        }
        public static GameObject[] GetChildren(this Transform parent)
        {
            GameObject[] children = new GameObject[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
                children[i] = parent.GetChild(i).gameObject;
            return children;
        }
        public static GameObject[] GetChildren(this GameObject parent)
        {
            var fatherTransform = parent.transform;
            GameObject[] children = new GameObject[fatherTransform.childCount];
            for (int i = 0; i < fatherTransform.childCount; i++)
                children[i] = fatherTransform.GetChild(i).gameObject;
            return children;
        }

        /// <summary>
        /// bool为true返回1,否则返回0
        /// </summary>
        public static int ToInt(this bool value)
        {
            return value? 1 : 0; 
        }

        /// <summary>
        /// Remove的O(1)版本,会破坏元素顺序,仅适用于无序列表
        /// </summary>
        public static void UnorderRemove<T>(this List<T> list,T item)
        {
            for (int i = 0; i < list.Count; i++)
                if(EqualityComparer<T>.Default.Equals(list[i], item))
                    UnorderRemoveAt(list,i);
        }
        /// <summary>
        /// RemoveAt的O(1)版本,会破坏元素顺序,仅适用于无序列表
        /// </summary>
        public static void UnorderRemoveAt<T>(this List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            list[index] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }
        public static T PopLast<T>(this List<T> list)
        {
            T target = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return target;
        }
    }
}
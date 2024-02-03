using System.Collections.Generic;
using UnityEngine;
namespace Paltry.Extension
{
    public static class Extension
    {
        public static GameObject[] GetChildren(this Transform father)
        {
            GameObject[] children = new GameObject[father.childCount];
            for (int i = 0; i < father.childCount; i++)
                children[i] = father.GetChild(i).gameObject;
            return children;
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
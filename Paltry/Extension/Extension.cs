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
        /// RemoveAt��O(1)�汾,���ƻ�Ԫ��˳��,�������������б�
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
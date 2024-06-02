using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    public static class MathUtil 
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// ��[0,0.5]����ӳ�䵽[0,1],[0.5,1]����ӳ�䵽[1,0]
        /// </summary>
        public static float Map01_010(float x)
        {
            return 2 * (x < 0.5f ? x : Ceilgap(x));
        }
        /// <summary>
        /// ��ȡһ��������������һ�������������,eg:4.8 => 0.2 -5.7=>0.7
        /// </summary>
        public static float Ceilgap(float x)
        {
            return x >= 0 ? 1 - x%1 : -x % 1;
        }
    }
}

